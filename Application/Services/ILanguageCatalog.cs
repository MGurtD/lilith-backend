namespace Application.Services;

using Application.Contracts.Shared;

public interface ILanguageCatalog
{
    Task<IEnumerable<LanguageDto>> GetAllAsync();
    Task<LanguageDto?> GetByCodeAsync(string code);
    Task<LanguageDto?> GetDefaultAsync();
}
