using Domain.Entities.Production;

namespace Application.Persistance.Repositories.Production
{
    public interface IWorkOrderRepository : IRepository<WorkOrder, Guid>
    {
        
        IWorkOrderPhaseRepository Phases { get; }

    }
}
