using Api.Services;
using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Api.Setup
{
    public static class LocalizationSetup
    {
        public static IServiceCollection AddLocalizationSetup(this IServiceCollection services)
        {
            // Configure localization options
            services.Configure<LocalizationOptions>(options =>
            {
                options.ResourcesPath = "Resources";
            });

            // Register custom JSON localizer factory
            services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();
            
            // Register localization service
            services.AddScoped<Application.Services.ILocalizationService, LocalizationService>();

            // Configure supported cultures
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("ca"),    // Catalan (default)
                    new CultureInfo("es"),    // Spanish
                    new CultureInfo("en")     // English
                };

                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture("ca");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            return services;
        }

        public static IApplicationBuilder UseLocalizationSetup(this IApplicationBuilder app)
        {
            // Use custom culture middleware before request localization
            app.UseMiddleware<Api.Middlewares.CultureMiddleware>();
            
            // Use built-in request localization
            app.UseRequestLocalization();

            return app;
        }
    }
}