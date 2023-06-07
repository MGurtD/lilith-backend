using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Persistance.Repositories.Sales;
using Domain.Entities.Sales;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class CustomerTypeRepository : Repository<CustomerType, Guid>, ICustomerTypeRepository
    {
        public CustomerTypeRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
