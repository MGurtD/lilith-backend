using System.Globalization;

namespace Api.Middlewares
{
    public class CultureMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _supportedCultures = { "ca", "es", "en" };

        public CultureMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var culture = DetermineCultureFromRequest(context);
            SetCulture(culture);
            
            await _next(context);
        }

        private string DetermineCultureFromRequest(HttpContext context)
        {
            // Priority order:
            // 1. Query parameter (?culture=ca)
            // 2. Header (Accept-Language)
            // 3. Default to Catalan

            // Check query parameter
            if (context.Request.Query.TryGetValue("culture", out var cultureQuery))
            {
                var requestedCulture = cultureQuery.ToString().ToLowerInvariant();
                if (_supportedCultures.Contains(requestedCulture))
                {
                    return requestedCulture;
                }
            }

            // Check Accept-Language header
            var acceptLanguageHeader = context.Request.Headers["Accept-Language"].ToString();
            if (!string.IsNullOrEmpty(acceptLanguageHeader))
            {
                var languages = acceptLanguageHeader.Split(',')
                    .Select(lang => lang.Split(';')[0].Trim().ToLowerInvariant())
                    .ToArray();

                foreach (var language in languages)
                {
                    // Check exact match first
                    if (_supportedCultures.Contains(language))
                    {
                        return language;
                    }

                    // Check language part (e.g., "ca" from "ca-ES")
                    var languageCode = language.Split('-')[0];
                    if (_supportedCultures.Contains(languageCode))
                    {
                        return languageCode;
                    }
                }
            }

            // Default to Catalan
            return "ca";
        }

        private static void SetCulture(string culture)
        {
            var cultureInfo = new CultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
            Thread.CurrentThread.CurrentCulture = cultureInfo;
            Thread.CurrentThread.CurrentUICulture = cultureInfo;
        }
    }
}