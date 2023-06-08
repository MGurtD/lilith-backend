using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class SiteRepository : Repository<Site, Guid>, ISiteRepository
    {
        public SiteRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
