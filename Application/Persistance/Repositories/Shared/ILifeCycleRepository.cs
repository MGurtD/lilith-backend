using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface ILifecycleRepository : IRepository<Lifecycle, Guid>
    {
        IStatusRepository StatusRepository { get; }
    }
}
