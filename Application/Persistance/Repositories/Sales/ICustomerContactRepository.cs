using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Sales;

namespace Application.Persistance.Repositories.Sales
{
    public interface ICustomerContactRepository : IRepository<CustomerContact, Guid>
    {
    }
}
