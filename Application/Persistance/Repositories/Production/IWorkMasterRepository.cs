using Domain.Entities.Production;

namespace Application.Persistance.Repositories.Production
{
    public interface IWorkMasterRepository : IRepository<WorkMaster, Guid>
    {
        IWorkMasterPhaseRepository Phases { get; }
    }
}
