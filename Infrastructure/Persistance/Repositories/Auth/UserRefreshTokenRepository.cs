using Application.Persistance.Repositories.Auth;
using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Auth
{
    public class UserRefreshTokenRepository : Repository<UserRefreshToken, Guid>, IUserRefreshTokenRepository
    {
        public UserRefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
