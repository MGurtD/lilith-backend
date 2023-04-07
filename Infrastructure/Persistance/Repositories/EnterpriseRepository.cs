using Application.Persistance;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories
{
    public class EnterpriseRepository : Repository<Enterprise, Guid>, IEnterpriseRepository
    {
        public EnterpriseRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<Enterprise?> Get(Guid id)
        {
            return await dbSet.Include("Sites").FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
