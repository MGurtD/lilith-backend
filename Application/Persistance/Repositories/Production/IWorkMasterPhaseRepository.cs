using Domain.Entities.Production;

namespace Application.Persistance.Repositories.Production
{
    public interface IWorkMasterPhaseRepository : IRepository<WorkMasterPhase, Guid>
    {
        IRepository<WorkMasterPhaseDetail, Guid> Details { get; }
        IRepository<WorkMasterPhaseBillOfMaterials, Guid> BillOfMaterials { get; }
    }
}
