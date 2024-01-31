using Domain.Entities.Production;

namespace Application.Persistance.Repositories.Production
{
    public interface IWorkOrderPhaseRepository : IRepository<WorkOrderPhase, Guid>
    {
        IRepository<WorkOrderPhaseDetail, Guid> Details { get; }
        IRepository<WorkOrderPhaseBillOfMaterials, Guid> BillOfMaterials { get; }
    }
}
