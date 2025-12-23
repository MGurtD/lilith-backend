using Domain.Entities.Production;

namespace Application.Contracts;

public interface IWorkcenterRepository : IRepository<Workcenter, Guid>
{
    Task<IEnumerable<Workcenter>> GetVisibleInPlant();
}