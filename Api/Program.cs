using Api;
using Api.Middlewares;
using Api.Setup;
using NLog;
using NLog.Web;
using System.Text.Json.Serialization;
using Application.Services; // ILanguageCatalog
using Api.Services; // LanguageCatalog

// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    {
        // Logging with NLog
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        // Configuration and settings
        builder.Services.Configure<AppSettings>(builder.Configuration);
        var appSettings = builder.Configuration.Get<AppSettings>()!;
        appSettings.Validate();

        // Set server request body size limit
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = appSettings.FileManagment.LimitSize * 1024L * 1024L;
        });

        builder.Services
            .AddDatabaseServices(appSettings.ConnectionStrings.Default)
            .AddApplicationServices()
            .AddJwtSetup(builder.Environment.IsDevelopment(), appSettings.JwtConfig.Secret)
            .AddSwaggerSetup()
            .AddLocalizationSetup(); // Add localization services

        // Language catalog singleton
        builder.Services.AddSingleton<ILanguageCatalog, LanguageCatalog>();

        builder.Services.AddControllers()
                        .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    }

    var app = builder.Build();
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        // Ensure authentication happens before culture resolution so we can read the user's locale claim
        app.UseAuthentication();
        app.UseLocalizationSetup();
        app.UseAuthorization();

        // middlewares
        app.UseMiddleware<HttpLoggingMiddleware>();
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // global cors policy
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        app.MapControllers();

        app.Run();
    }
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}