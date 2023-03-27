namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        ICustomerRepository Customers { get; }
        IOperatorRepository Operators { get; }

        int Complete();
    }
}
