using Application.Contracts;
using Domain.Entities.Shared;

namespace Application.Services
{
    public interface ILanguageService
    {
        Task<GenericResponse> GetAllLanguagesAsync();
        Task<GenericResponse> GetLanguageByCodeAsync(string code);
        Task<GenericResponse> GetDefaultLanguageAsync();
        Task<GenericResponse> CreateLanguageAsync(Language language);
        Task<GenericResponse> UpdateLanguageAsync(Guid id, Language language);
        Task<GenericResponse> DeleteLanguageAsync(Guid id);
    }
}