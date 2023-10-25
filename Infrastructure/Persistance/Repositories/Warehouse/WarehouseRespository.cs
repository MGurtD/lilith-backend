using Application.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Application.Persistance.Repositories.Warehouse;
using Domain.Entities.Warehouse;

namespace Infrastructure.Persistance.Repositories.Warehouse
{
    public class WarehouseRepository : Repository<Domain.Entities.Warehouse.Warehouse, Guid>, IWarehouseRepository
    {
        public IRepository<Location, Guid> Locations { get; }

        public WarehouseRepository(ApplicationDbContext context) : base(context)
        {
            Locations = new Repository<Location, Guid>(context);
        }

        public override async Task<Domain.Entities.Warehouse.Warehouse?> Get(Guid id)
        {
            return await dbSet
                        .Include(w => w.Locations)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

    }
}
