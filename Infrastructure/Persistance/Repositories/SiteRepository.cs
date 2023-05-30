using Application.Persistance.Repositories;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class SiteRepository : Repository<Site, Guid>, ISiteRepository
    {
        public SiteRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
