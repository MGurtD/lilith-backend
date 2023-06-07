using Domain.Entities.Sales;

namespace Application.Persistance.Repositories.Sales
{
    public interface ICustomerAddressRepository : IRepository<CustomerAddress, Guid>
    {
    }
}
