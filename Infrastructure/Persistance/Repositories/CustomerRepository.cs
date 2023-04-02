using Application.Persistance;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class CustomerRepository : Repository<Customer, Guid>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        // TODO: Remove
        public IEnumerable<Customer> GetTopCustomers()
        {
            return dbSet.OrderBy(c => c.Name).Take(10);
        }
    }
}
