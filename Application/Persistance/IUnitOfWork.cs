namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        IEnterpriseRepository Enterprises { get; }
        ISiteRepository Sites { get; }
        IRoleRepository Roles { get; }
        IUserRepository Users { get; }
        IUserRefreshTokenRepository UserRefreshTokens { get; }
        ISupplierTypeRepository SupplierTypes { get; }
        ISupplierRepository Suppliers { get; }
        ICustomerTypeRepository CustomerTypes { get; }
        ICustomerContactRepository CustomerContacts { get; }
        ICustomerAddressRepository CustomerAddresses { get; }
        ICustomerRepository Customers { get; }

        Task<int> CompleteAsync();
    }
}
