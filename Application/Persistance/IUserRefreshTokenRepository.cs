using Domain.Entities;

namespace Application.Persistance
{
    public interface IUserRefreshTokenRepository : IRepository<UserRefreshToken, Guid>
    {
    }
}
