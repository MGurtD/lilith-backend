using Application.Persistance;
using Infrastructure.Persistance.Repositories;

namespace Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public readonly ApplicationDbContext context;
        public IEnterpriseRepository Enterprises { get; private set; }
        public ISiteRepository Sites { get; private set; }        
        public IRoleRepository Roles { get; private set; }
        public IUserRepository Users { get; private set; }
        public IUserRefreshTokenRepository UserRefreshTokens { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            UserRefreshTokens = new UserRefreshTokenRepository(context);
            Enterprises = new EnterpriseRepository(context);
            Sites = new SiteRepository(context);
            Users = new UserRepository(context);
            Roles = new RoleRepository(context);
        }

        public async Task<int> CompleteAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
