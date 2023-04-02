namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        IEnterpriseRepository Enterprises { get; }

        int Complete();
    }
}
