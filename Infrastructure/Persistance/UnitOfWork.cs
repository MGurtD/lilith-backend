using Application.Persistance;
using Infrastructure.Persistance.Repositories;

namespace Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public readonly ApplicationDbContext context;
        public ICustomerRepository Customers { get; private set; }
        public IEnterpriseRepository Enterprises { get; private set; }
        public ISiteRepository Sites { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            Customers = new CustomerRepository(context);
            Enterprises = new EnterpriseRepository(context);
            Sites = new SiteRepository(context);
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
