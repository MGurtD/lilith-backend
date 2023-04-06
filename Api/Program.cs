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
using Api.Mapping;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    {
        builder.Configuration.AddUserSecrets("Lilith.Backend").AddEnvironmentVariables();

        // Logging with NLog
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        // Database Context
        builder.Services.AddDbContext<ApplicationDbContext>(options => {
            options.UseNpgsql(GetConnectionString(builder.Configuration));
        });
        builder.Services
            .AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedEmail = false)
            .AddEntityFrameworkStores<ApplicationDbContext>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

        // JWT Service    
        builder.Services
            .AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(jwt =>
            {
                var signKey = Encoding.ASCII.GetBytes(GetJwtToken(builder.Configuration));

                jwt.SaveToken = true; // After authentication, token will be saved
                jwt.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(signKey),
                    ValidateIssuer = !builder.Environment.IsDevelopment(),
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = !builder.Environment.IsDevelopment(),
                    RequireExpirationTime = !builder.Environment.IsDevelopment(),
                    ValidateLifetime = true,
                };
            });

        builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
        builder.Services.AddControllers();

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
    NLog.LogManager.Shutdown();
}

static string GetConnectionString(ConfigurationManager configurationManager)
{
    var connectionString = configurationManager.GetConnectionString("Default");
    if (connectionString is null)
    {
        throw new Exception("ConnectionString:Default configuration is required");
    }

    return connectionString;
}

static string GetJwtToken(ConfigurationManager configurationManager)
{
    var jwtSecret = configurationManager.GetSection("JwtConfig:Secret").Value;
    if (jwtSecret is null)
    {
        throw new Exception("JwtConfig:Secret configuration is required");
    }

    return jwtSecret;
}