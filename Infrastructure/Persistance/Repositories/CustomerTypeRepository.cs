using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Persistance;
using Domain.Entities;

namespace Infrastructure.Persistance.Repositories
{
    public class CustomerTypeRepository : Repository<CustomerType, Guid>, ICustomerTypeRepository
    {
        public CustomerTypeRepository(ApplicationDbContext context) : base(context) 
        { 
        }
    }
}
