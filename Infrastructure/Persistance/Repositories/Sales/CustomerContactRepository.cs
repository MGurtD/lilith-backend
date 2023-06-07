using Application.Persistance.Repositories.Sales;
using Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class CustomerContactRepository : Repository<CustomerContact, Guid>, ICustomerContactRepository
    {
        public CustomerContactRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
