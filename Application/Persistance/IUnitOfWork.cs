namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        IUserRefreshTokenRepository UserRefreshTokens { get; }
        ICustomerRepository Customers { get; }
        IEnterpriseRepository Enterprises { get; }
        ISiteRepository Sites { get; }

        Task<int> CompleteAsync();
    }
}
