using Application.Persistance.Repositories.Sales;
using Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class CustomerAddressRepository : Repository<CustomerAddress, Guid>, ICustomerAddressRepository
    {
        public CustomerAddressRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
