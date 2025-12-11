using Domain.Entities.Production;

namespace Application.Persistance.Repositories.Production
{
    public interface IWorkOrderRepository : IRepository<WorkOrder, Guid>
    {
        
        IWorkOrderPhaseRepository Phases { get; }

        Task<WorkOrder?> GetDetailed(Guid id);
        Task<IEnumerable<WorkOrder>> GetByWorkcenterIdInProduction(Guid workcenterId, Guid productionStatusId);

    }
}
