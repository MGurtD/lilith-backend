using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface IFileRepository : IRepository<Domain.Entities.File, Guid>
    {
    }
}
