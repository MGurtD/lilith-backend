using Application.Persistance.Repositories.Auth;
using Domain.Entities.Auth;

namespace Infrastructure.Persistance.Repositories.Auth
{
    public class UserRepository : Repository<User, Guid>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
