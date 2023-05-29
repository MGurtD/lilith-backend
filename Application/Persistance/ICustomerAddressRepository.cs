using Application.Persistance;
using Domain.Entities;

namespace Application.Persistance
{
    public interface ICustomerAddressRepository : IRepository<CustomerAddress, Guid>
    {
    }
}
