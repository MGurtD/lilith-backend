using Application.Persistance;
using Infrastructure.Persistance.Repositories;

namespace Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public readonly ApplicationDbContext context;

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            Customers = new CustomerRepository(context);
            Operators = new OperatorRepository(context);
        }

        public ICustomerRepository Customers { get; private set; }

        public IOperatorRepository Operators { get; private set; }

        public int Complete()
        {
            return context.SaveChanges();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}
