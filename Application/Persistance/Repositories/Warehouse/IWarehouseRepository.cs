using Domain.Entities.Warehouse;

namespace Application.Persistance.Repositories.Warehouse
{
    public interface IWarehouseRepository : IRepository<Domain.Entities.Warehouse.Warehouse, Guid>
    {
        IRepository<Location, Guid> Locations { get; }
    }
}
