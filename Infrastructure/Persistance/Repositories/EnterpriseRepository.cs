using Application.Persistance;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class EnterpriseRepository : Repository<Enterprise, Guid>, IEnterpriseRepository
    {
        public EnterpriseRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
