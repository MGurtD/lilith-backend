using Application.Contracts;
using Application.Persistance;
using Application.Services;

namespace Api.Services
{
    public class LanguageService : ILanguageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;

        public LanguageService(IUnitOfWork unitOfWork, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
        }

        public async Task<GenericResponse> GetAllLanguagesAsync()
        {
            try
            {
                var languages = await _unitOfWork.Languages.GetActiveLanguagesAsync();
                return new GenericResponse(true, languages);
            }
            catch (Exception)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.ErrorRetrievingData"));
            }
        }

        public async Task<GenericResponse> GetLanguageByCodeAsync(string code)
        {
            try
            {
                var language = await _unitOfWork.Languages.GetByCodeAsync(code);
                if (language == null)
                {
                    return new GenericResponse(false, _localizationService.GetLocalizedString("LanguageNotFound", code));
                }

                return new GenericResponse(true, language);
            }
            catch (Exception)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.ErrorRetrievingData"));
            }
        }

        public async Task<GenericResponse> GetDefaultLanguageAsync()
        {
            try
            {
                var language = await _unitOfWork.Languages.GetDefaultLanguageAsync();
                if (language == null)
                {
                    return new GenericResponse(false, _localizationService.GetLocalizedString("DefaultLanguageNotFound"));
                }

                return new GenericResponse(true, language);
            }
            catch (Exception)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.ErrorRetrievingData"));
            }
        }

        public async Task<GenericResponse> CreateLanguageAsync(Domain.Entities.Shared.Language language)
        {
            try
            {
                // Validation: Check if code already exists
                var existingLanguage = await _unitOfWork.Languages.GetByCodeAsync(language.Code);
                if (existingLanguage != null)
                {
                    return new GenericResponse(false, _localizationService.GetLocalizedString("LanguageCodeAlreadyExists", language.Code));
                }

                // If this is set as default, remove default from others
                if (language.IsDefault)
                {
                    var currentDefault = await _unitOfWork.Languages.GetDefaultLanguageAsync();
                    if (currentDefault != null)
                    {
                        currentDefault.IsDefault = false;
                        _unitOfWork.Languages.UpdateWithoutSave(currentDefault);
                    }
                }

                await _unitOfWork.Languages.Add(language);
                await _unitOfWork.CompleteAsync();

                return new GenericResponse(true, language);
            }
            catch (Exception)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.ErrorCreatingEntity"));
            }
        }

        public async Task<GenericResponse> UpdateLanguageAsync(Guid id, Domain.Entities.Shared.Language language)
        {
            try
            {
                var existingLanguage = await _unitOfWork.Languages.Get(id);
                if (existingLanguage == null)
                {
                    return new GenericResponse(false, _localizationService.GetLocalizedString("LanguageNotFound", id));
                }

                // Update properties
                existingLanguage.Name = language.Name;
                existingLanguage.Icon = language.Icon;
                existingLanguage.SortOrder = language.SortOrder;

                // Handle default language change
                if (language.IsDefault && !existingLanguage.IsDefault)
                {
                    var currentDefault = await _unitOfWork.Languages.GetDefaultLanguageAsync();
                    if (currentDefault != null && currentDefault.Id != id)
                    {
                        currentDefault.IsDefault = false;
                        _unitOfWork.Languages.UpdateWithoutSave(currentDefault);
                    }
                    existingLanguage.IsDefault = true;
                }
                else if (!language.IsDefault && existingLanguage.IsDefault)
                {
                    existingLanguage.IsDefault = false;
                }

                _unitOfWork.Languages.UpdateWithoutSave(existingLanguage);
                await _unitOfWork.CompleteAsync();

                return new GenericResponse(true, existingLanguage);
            }
            catch (Exception)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.ErrorUpdatingEntity"));
            }
        }

        public async Task<GenericResponse> DeleteLanguageAsync(Guid id)
        {
            try
            {
                var language = await _unitOfWork.Languages.Get(id);
                if (language == null)
                {
                    return new GenericResponse(false, _localizationService.GetLocalizedString("LanguageNotFound", id));
                }

                if (language.IsDefault)
                {
                    return new GenericResponse(false, _localizationService.GetLocalizedString("CannotDeleteDefaultLanguage"));
                }

                language.Disabled = true;
                _unitOfWork.Languages.UpdateWithoutSave(language);
                await _unitOfWork.CompleteAsync();

                return new GenericResponse(true, _localizationService.GetLocalizedString("EntityDeleted"));
            }
            catch (Exception)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("Common.ErrorDeletingEntity"));
            }
        }
    }
}