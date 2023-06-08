using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class EnterpriseRepository : Repository<Enterprise, Guid>, IEnterpriseRepository
    {
        public EnterpriseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Enterprise?> Get(Guid id)
        {
            return await dbSet.Include("Sites").AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public override async Task<IEnumerable<Enterprise>> GetAll()
        {
            return await dbSet.Include("Sites").AsNoTracking().ToListAsync();
        }

    }
}
