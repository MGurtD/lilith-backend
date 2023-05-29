using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance
{
    public interface ICustomerRepository : IRepository<Customer, Guid>
    {
        CustomerContact? GetContactById(Guid id);
        Task AddContact(CustomerContact contact);
        Task UpdateContact(CustomerContact contact);
        Task RemoveContact(CustomerContact contact);

        CustomerAddress? GetAddressById(Guid id);
        Task AddAddress(CustomerAddress address);
        Task UpdateAddress(CustomerAddress address);
        Task RemoveAddress(CustomerAddress address);
    }
}
