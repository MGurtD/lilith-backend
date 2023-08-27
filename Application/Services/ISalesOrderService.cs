using Application.Contracts;
using Application.Contracts.Sales;
using Domain.Entities.Sales;
using SalesOrderDetail = Domain.Entities.Sales.SalesOrderDetail;

namespace Application.Services
{
    public interface ISalesOrderService
    {
        Task<GenericResponse> Create(CreateOrderOrInvoiceRequest createRequest);

        Task<SalesOrderHeader?> GetById(Guid id);
        IEnumerable<SalesOrderHeader> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<SalesOrderHeader> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);

        Task<GenericResponse> Update(SalesOrderHeader salesOrderHeader);
        Task<GenericResponse> Remove(Guid id);

        Task<SalesOrderDetail?> GetDetailById(Guid id);
        Task<GenericResponse> AddDetail(SalesOrderDetail detail);
        Task<GenericResponse> UpdateDetail(SalesOrderDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);

    }
}
