using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.Json;

namespace Api.Services
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, Dictionary<string, string>> _localizations = new();
        private readonly string _resourcesPath;
        private readonly string _baseName;

        public JsonStringLocalizer(string resourcesPath, string baseName)
        {
            _resourcesPath = resourcesPath;
            _baseName = baseName;
            LoadLocalizations();
        }

        public LocalizedString this[string name] => GetLocalizedString(name);

        public LocalizedString this[string name, params object[] arguments] => GetLocalizedString(name, arguments);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            
            if (_localizations.TryGetValue(culture, out var strings))
            {
                return strings.Select(s => new LocalizedString(s.Key, s.Value, false));
            }

            return Enumerable.Empty<LocalizedString>();
        }

        private LocalizedString GetLocalizedString(string name, params object[] arguments)
        {
            var culture = CultureInfo.CurrentUICulture.Name;
            
            // Try exact culture match first (e.g., "ca-ES")
            if (_localizations.TryGetValue(culture, out var cultureStrings))
            {
                if (cultureStrings.TryGetValue(name, out var value))
                {
                    return new LocalizedString(name, FormatString(value, arguments), false);
                }
            }

            // Try language-only match (e.g., "ca" for "ca-ES")
            var languageCode = culture.Split('-')[0];
            if (_localizations.TryGetValue(languageCode, out var languageStrings))
            {
                if (languageStrings.TryGetValue(name, out var value))
                {
                    return new LocalizedString(name, FormatString(value, arguments), false);
                }
            }

            // Fall back to English if available
            if (_localizations.TryGetValue("en", out var englishStrings))
            {
                if (englishStrings.TryGetValue(name, out var value))
                {
                    return new LocalizedString(name, FormatString(value, arguments), false);
                }
            }

            // Return the key itself if not found
            return new LocalizedString(name, name, true);
        }

        private static string FormatString(string template, params object[] arguments)
        {
            if (arguments?.Length > 0)
            {
                try
                {
                    return string.Format(template, arguments);
                }
                catch
                {
                    return template;
                }
            }
            return template;
        }

        private void LoadLocalizations()
        {
            // The baseName is "LocalizationService", so we look for files in Resources/LocalizationService/
            var resourcePath = Path.Combine(_resourcesPath, _baseName);
            
            if (!Directory.Exists(resourcePath))
            {
                // Log that the directory doesn't exist for debugging
                Console.WriteLine($"Resource directory not found: {Path.GetFullPath(resourcePath)}");
                return;
            }

            var jsonFiles = Directory.GetFiles(resourcePath, "*.json");
            Console.WriteLine($"Found {jsonFiles.Length} localization files in {resourcePath}");
            
            foreach (var file in jsonFiles)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var culture = fileName; // e.g., "ca.json" -> "ca"

                try
                {
                    var json = File.ReadAllText(file);
                    var localizations = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    
                    if (localizations != null)
                    {
                        _localizations[culture] = localizations;
                        Console.WriteLine($"Loaded {localizations.Count} translations for culture '{culture}'");
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue loading other files
                    Console.WriteLine($"Error loading localization file {file}: {ex.Message}");
                }
            }
        }
    }
}