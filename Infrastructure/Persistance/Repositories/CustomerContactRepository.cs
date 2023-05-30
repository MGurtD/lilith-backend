using Application.Persistance.Repositories;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.Repositories
{
    public class CustomerContactRepository : Repository<CustomerContact, Guid>, ICustomerContactRepository
    {
        public CustomerContactRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
