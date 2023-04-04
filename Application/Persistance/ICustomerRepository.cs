using Domain.Entities;

namespace Application.Persistance
{
    public interface ICustomerRepository : IRepository<Customer, Guid>
    {
    }
}
