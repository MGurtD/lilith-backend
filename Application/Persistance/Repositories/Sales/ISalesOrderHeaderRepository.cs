using Domain.Entities.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Persistance.Repositories.Sales
{
    public interface ISalesOrderHeaderRepository : IRepository<SalesOrderHeader, Guid>
    {
        SalesOrderDetail? GetDetailById(Guid id);
        Task AddDetail(SalesOrderDetail detail);
        Task UpdateDetail(SalesOrderDetail detail);
        Task RemoveDetail(SalesOrderDetail detail);
    }
}

