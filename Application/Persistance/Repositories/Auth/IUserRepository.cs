using Domain.Entities.Auth;

namespace Application.Persistance.Repositories.Auth
{
    public interface IUserRepository : IRepository<User, Guid>
    {
    }
}
