using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface IUserRepository : IRepository<User, Guid>
    {
    }

    public interface IRoleRepository : IRepository<Role, Guid>
    {
    }
}
