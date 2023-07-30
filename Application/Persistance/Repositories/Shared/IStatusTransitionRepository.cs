using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface IStatusTransitionRepository : IRepository<StatusTransition, Guid>
    {
    }
}
