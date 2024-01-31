using Application.Persistance;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Text;
using NLog;
using NLog.Web;
using Api.Middlewares;
using Api;
using Application.Services;
using Api.Services;
using System.Text.Json.Serialization;
using Application.Services.Sales;
using Api.Services.Sales;
using Application.Services.Warehouse;
using Application.Services.Purchase;
using Api.Services.Purchase;
using Api.Services.Warehouse;
using Application.Production.Warehouse;
using Application.Services.Production;
using Api.Services.Production;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    {
        Configuration.Load(builder.Configuration);

        // Logging with NLog
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        // Database Context
        builder.Services.AddDbContext<ApplicationDbContext>(options => {
            options.UseNpgsql(Configuration.ConnectionString);
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedEmail = false)
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Application services
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<IFileService, FileService>();
        builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
        builder.Services.AddScoped<ISalesOrderService, SalesOrderService>();
        builder.Services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
        builder.Services.AddScoped<IDueDateService, DueDateService>();
        builder.Services.AddScoped<IStockService, StockService>();
        builder.Services.AddScoped<IStockMovementService, StockMovementService>();
        builder.Services.AddScoped<IDeliveryNoteService, DeliveryNoteService>();
        builder.Services.AddScoped<IReceiptService, ReceiptService>();
        builder.Services.AddScoped<IReferenceService, ReferenceService>();
        builder.Services.AddScoped<IExerciseService, ExerciseService>();
        builder.Services.AddScoped<IWorkMasterService, WorkMasterService>();
        builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();

        // JWT Service    
        var signKey = Encoding.ASCII.GetBytes(Configuration.JwtSecret);
        var tokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(signKey),
            ValidateIssuer = !builder.Environment.IsDevelopment(),
            ValidateIssuerSigningKey = true,
            ValidateAudience = !builder.Environment.IsDevelopment(),
            RequireExpirationTime = !builder.Environment.IsDevelopment(),
            ValidateLifetime = true,
        };
        builder.Services
            .AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                jwt.SaveToken = true; // After authentication, token will be saved
                jwt.TokenValidationParameters = tokenValidationParameters;
            });
        builder.Services.AddSingleton(tokenValidationParameters);

        builder.Services.AddControllers()
                        .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Description = "Standard Authorization header using the Bearer scheme"
            });

            options.OperationFilter<SecurityRequirementsOperationFilter>();
        });
    }

    var app = builder.Build();
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        // global cors policy
        app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

        // global error handler
        app.UseMiddleware<ErrorHandlerMiddleware>();

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