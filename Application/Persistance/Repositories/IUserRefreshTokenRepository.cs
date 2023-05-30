using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface IUserRefreshTokenRepository : IRepository<UserRefreshToken, Guid>
    {
    }
}
