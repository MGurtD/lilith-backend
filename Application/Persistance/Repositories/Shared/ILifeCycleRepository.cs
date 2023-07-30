using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface ILifeCycleRepository : IRepository<Lifecycle, Guid>
    {
        IStatusRepository StatusRepository { get; }
    }
}
