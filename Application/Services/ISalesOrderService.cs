using Application.Contracts.Auth;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public interface ISalesOrderService
    {
        Task<GenericResponse> Create(SalesOrderHeader salesOrderHeader);
        Task<SalesOrderHeader?> GetById(Guid id);
        IEnumerable<SalesOrderHeader> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<SalesOrderHeader> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);

        Task<GenericResponse> Update(SalesOrderHeader salesOrderHeader);
        Task<GenericResponse> Remove(Guid id);  
        Task<GenericResponse>AddDetail(SalesOrderDetail detail);
        Task<GenericResponse> UpdateDetail(SalesOrderDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);

    }
}
