using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class AreaRepository : Repository<Area, Guid>, IAreaRepository
    {
        public AreaRepository(ApplicationDbContext context) : base(context) { } 
    }
}
