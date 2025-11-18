using Domain.Entities.Production;

namespace Application.Persistance.Repositories.Production;

public interface IMachineStatusRepository : IRepository<MachineStatus, Guid>
{
    IRepository<MachineStatusReason, Guid> Reasons { get; }
}
