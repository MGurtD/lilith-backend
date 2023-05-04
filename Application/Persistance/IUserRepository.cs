using Domain.Entities;

namespace Application.Persistance
{
    public interface IUserRepository : IRepository<User, Guid>
    {
    }

    public interface IRoleRepository : IRepository<Role, Guid>
    {
    }
}
