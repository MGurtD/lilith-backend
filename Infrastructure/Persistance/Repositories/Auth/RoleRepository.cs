using Application.Persistance.Repositories.Auth;
using Domain.Entities.Auth;

namespace Infrastructure.Persistance.Repositories.Auth
{
    public class RoleRepository : Repository<Role, Guid>, IRoleRepository
    {
        public RoleRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
