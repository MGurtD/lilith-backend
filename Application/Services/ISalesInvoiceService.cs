using Application.Contracts;
using Application.Contracts.Sales;
using Domain.Entities.Sales;

namespace Application.Services
{
    public interface ISalesInvoiceService
    {
        Task<GenericResponse> Create(CreateInvoiceRequest createInvoiceRequest);        

        Task<SalesInvoice?> GetById(Guid id);
        IEnumerable<SalesInvoice> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<SalesInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
        IEnumerable<SalesInvoice> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);
        IEnumerable<SalesInvoice> GetByCustomer(Guid customerId);
        IEnumerable<SalesInvoice> GetByStatus(Guid statusId);
        IEnumerable<SalesInvoice> GetByExercise(Guid exerciseId);

        Task<GenericResponse> Update(SalesInvoice SalesInvoice);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> AddDetailsFromOrderDetails(SalesInvoice salesInvoice, IEnumerable<Domain.Entities.Sales.SalesOrderDetail> salesOrderDetails);
        Task<GenericResponse> AddDetail(SalesInvoiceDetail detail);
        Task<GenericResponse> UpdateDetail(SalesInvoiceDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);
    }
}