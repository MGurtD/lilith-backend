using Domain.Entities.Auth;

namespace Application.Persistance.Repositories.Auth
{
    public interface IRoleRepository : IRepository<Role, Guid>
    {
    }
}
