using Microsoft.Extensions.Localization;
using System.Globalization;

namespace Api.Services
{
    public class LocalizationService : Application.Services.ILocalizationService
    {
        private readonly IStringLocalizerFactory _localizerFactory;
        private readonly IStringLocalizer _localizer;

        public LocalizationService(IStringLocalizerFactory localizerFactory)
        public LocalizationService(IStringLocalizerFactory localizerFactory, ICultureService cultureService)
        {
            _localizerFactory = localizerFactory;
            _localizer = _localizerFactory.Create("LocalizationService", string.Empty);
        }

        public string GetLocalizedString(string key, params object[] arguments)
        {
            var localizedString = _localizer[key, arguments];
            return localizedString.ResourceNotFound ? key : localizedString.Value;
        }

        public string GetLocalizedStringForCulture(string key, string culture, params object[] arguments)
        {
            var currentCulture = CultureInfo.CurrentCulture;
            var currentUICulture = CultureInfo.CurrentUICulture;

            try
            {
                var cultureInfo = new CultureInfo(culture);
                CultureInfo.CurrentCulture = cultureInfo;
                CultureInfo.CurrentUICulture = cultureInfo;

                return GetLocalizedString(key, arguments);
            }
            finally
            {
                CultureInfo.CurrentCulture = currentCulture;
                CultureInfo.CurrentUICulture = currentUICulture;
            }
        }
    }
}