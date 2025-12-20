using Application.Persistance.Repositories;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Domain.Repositories.Shared;

public interface ILifecycleTagRepository : IRepository<LifecycleTag, Guid>
{
    Task<List<LifecycleTag>> GetByLifecycleId(Guid lifecycleId);
    Task<List<Status>> GetStatusesByTag(Guid lifecycleTagId);
    Task<bool> ExistsInLifecycle(string tagName, Guid lifecycleId, Guid? excludeId = null);
    Task<List<LifecycleTag>> GetTagsByStatus(Guid statusId);
}
