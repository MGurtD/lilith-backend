namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        IEnterpriseRepository Enterprises { get; }
        ISiteRepository Sites { get; }

        Task<int> CompleteAsync();
    }
}
