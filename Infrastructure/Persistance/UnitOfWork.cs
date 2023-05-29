using Application.Persistance;
using Domain.Entities;
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
        public ISupplierTypeRepository SupplierTypes { get; private set; }

        public ISupplierRepository Suppliers { get; private set; }

        public ICustomerTypeRepository CustomerTypes { get; private set; }
        public ICustomerAddressRepository CustomerAddresses { get; private set; }
        public ICustomerContactRepository CustomerContacts { get; private set; }
        public ICustomerRepository Customers { get; private set; }
        

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            UserRefreshTokens = new UserRefreshTokenRepository(context);
            Enterprises = new EnterpriseRepository(context);
            Sites = new SiteRepository(context);
            Users = new UserRepository(context);
            Roles = new RoleRepository(context);
            SupplierTypes = new SupplierTypeRepository(context);
            Suppliers =  new SupplierRepository(context, new SupplierContactRepository(context));
            CustomerTypes = new CustomerTypeRepository(context);
            CustomerAddresses = new CustomerAddressRepository(context);
            CustomerContacts = new CustomerContactRepository(context);
            Customers = new CustomerRepository(context, new CustomerContactRepository(context), new CustomerAddressRepository(context));
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
