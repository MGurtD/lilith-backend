using Domain.Entities.Production;

namespace Application.Persistance.Repositories.Production;

public interface IAreaRepository : IRepository<Area, Guid>
{
    Task<IEnumerable<Area>> GetVisibleInPlantWithWorkcenters();
}
