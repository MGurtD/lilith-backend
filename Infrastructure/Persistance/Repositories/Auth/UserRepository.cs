using Application.Persistance.Repositories.Auth;
using Domain.Entities;
using Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace Infrastructure.Persistance.Repositories.Auth
{
    public class UserRepository : Repository<User, Guid>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        { }

        public override async Task<User?> Get(Guid id)
        {
            return await dbSet.Include(u => u.Role).FirstAsync(u => u.Id == id);
        }
    }
    
}
