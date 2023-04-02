namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }

        int Complete();
    }
}
