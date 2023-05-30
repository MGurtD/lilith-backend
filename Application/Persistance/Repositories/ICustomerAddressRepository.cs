using Domain.Entities;

namespace Application.Persistance.Repositories
{
    public interface ICustomerAddressRepository : IRepository<CustomerAddress, Guid>
    {
    }
}
