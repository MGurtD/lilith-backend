using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface IStatusRepository : IRepository<Status, Guid>
    {
        IRepository<StatusTransition, Guid> TransitionRepository { get; }

        Task AddTransition(StatusTransition transition);
        Task UpdateTransition(StatusTransition transition);
        Task<bool> RemoveTransition(StatusTransition transition);
    }
}
