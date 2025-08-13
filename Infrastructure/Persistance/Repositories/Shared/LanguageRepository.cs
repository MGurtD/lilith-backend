using Domain.Entities.Shared;
using Domain.Repositories.Shared;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Shared
{
    public class LanguageRepository : Repository<Language, Guid>, ILanguageRepository
    {
        public LanguageRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Language?> GetByCodeAsync(string code)
        {
            return await context.Set<Language>()
                .Where(l => l.Code == code && !l.Disabled)
                .FirstOrDefaultAsync();
        }

        public async Task<Language?> GetDefaultLanguageAsync()
        {
            return await context.Set<Language>()
                .Where(l => l.IsDefault && !l.Disabled)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Language>> GetActiveLanguagesAsync()
        {
            return await context.Set<Language>()
                .Where(l => !l.Disabled)
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.Name)
                .ToListAsync();
        }
    }
}