using Domain.Entities.Shared;
using Application.Persistance.Repositories;

namespace Domain.Repositories.Shared
{
    public interface ILanguageRepository : IRepository<Language, Guid>
    {
        Task<Language?> GetByCodeAsync(string code);
        Task<Language?> GetDefaultLanguageAsync();
        Task<IEnumerable<Language>> GetActiveLanguagesAsync();
    }
}