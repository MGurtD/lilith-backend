using Application.Contracts;
using Domain.Entities.Purchase;

namespace Application.Services
{
    public interface IPurchaseInvoiceService
    {
        IEnumerable<PurchaseInvoice> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<PurchaseInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
        IEnumerable<PurchaseInvoice> GetBetweenDatesAndSupplier(DateTime startDate, DateTime endDate, Guid supplierId);
        Task<IEnumerable<PurchaseInvoice>> GetByExercise(Guid exerciseId);

        Task<IEnumerable<PurchaseInvoiceDueDate>?> GetPurchaseInvoiceDueDates(PurchaseInvoice purchaseInvoice);

        Task<GenericResponse> Create(PurchaseInvoice purchaseInvoice);

        Task<GenericResponse> ChangeStatus(Guid id, Guid toStatusId);

        Task<GenericResponse> ChangeStatuses(List<PurchaseInvoice> purchaseInvoices, Guid toStatusId);


    }
}