using Application.Contracts;
using Application.Contracts.Purchase;
using Domain.Entities.Sales;

namespace Application.Services
{
    public interface ISalesInvoiceService
    {
        Task<GenericResponse> Create(SalesInvoice SalesInvoice);

        Task<SalesInvoice?> GetById(Guid id);
        IEnumerable<SalesInvoice> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<SalesInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
        IEnumerable<SalesInvoice> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId);
        IEnumerable<SalesInvoice> GetByCustomer(Guid customerId);
        IEnumerable<SalesInvoice> GetByStatus(Guid statusId);
        IEnumerable<SalesInvoice> GetByExercise(Guid exerciseId);

        Task<GenericResponse> RecreateDueDates(SalesInvoice SalesInvoice);
        Task<GenericResponse> ChangeStatus(ChangeStatusRequest changeStatusRequest);
        Task<GenericResponse> Update(SalesInvoice SalesInvoice);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> AddImport(SalesInvoiceImport import);
        Task<GenericResponse> UpdateImport(SalesInvoiceImport import);
        Task<GenericResponse> RemoveImport(Guid id);

        Task<GenericResponse> AddDetail(SalesInvoiceDetail detail);
        Task<GenericResponse> UpdateDetail(SalesInvoiceDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);
    }
}