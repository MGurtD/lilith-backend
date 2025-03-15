using Api.Middlewares;
using Api.Setup;
using NLog;
using NLog.Web;
using System.Text.Json.Serialization;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    {
        Settings.Load(builder.Configuration);

        // Logging with NLog
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        // Set server request body size limit
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = Settings.FileUploadLimitSize;
        });

        builder.Services
            .AddDatabaseServices()
            .AddApplicationServices()
            .AddJwtSetup(builder.Environment.IsDevelopment())
            .AddSwaggerSetup();

        builder.Services.AddControllers()
                        .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
    }

    var app = builder.Build();
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthentication();
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