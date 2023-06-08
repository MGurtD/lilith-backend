using Domain.Entities.Auth;

namespace Application.Persistance.Repositories.Auth
{
    public interface IUserRefreshTokenRepository : IRepository<UserRefreshToken, Guid>
    {
    }
}
