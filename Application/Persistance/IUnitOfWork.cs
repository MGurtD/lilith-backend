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

        Task<int> CompleteAsync();
    }
}
