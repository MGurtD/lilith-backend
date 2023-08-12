using Application.Contracts;
using Domain.Entities.Sales;

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

        Task<GenericResponse> AddDetail(SalesOrderDetail detail);
        Task<GenericResponse> UpdateDetail(SalesOrderDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);

    }
}
