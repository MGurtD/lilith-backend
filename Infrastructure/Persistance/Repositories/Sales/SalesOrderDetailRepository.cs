using Application.Persistance.Repositories.Sales;
using Application.Persistance.Repositories;
using Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class SalesOrderDetailRepository : Repository<SalesOrderDetail, Guid>, ISalesOrderDetailRepository
    {
        public SalesOrderDetailRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
