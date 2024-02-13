using Application.Contracts;
using Application.Contracts.Purchase;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase
{
    public interface IReceiptService
    {
        IEnumerable<Receipt> GetBetweenDates(DateTime startDate, DateTime endDate);
        IEnumerable<Receipt> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId);
        IEnumerable<Receipt> GetBetweenDatesAndSupplier(DateTime startDate, DateTime endDate, Guid customerId);
        IEnumerable<Receipt> GetBySupplier(Guid supplierId, bool withoutInvoice);
        IEnumerable<Receipt> GetByInvoice(Guid invoiceId);
        IEnumerable<Receipt> GetByStatus(Guid statusId);

        Task<GenericResponse> Create(CreateReceiptRequest createRequest);
        Task<GenericResponse> Update(Receipt receipt);
        Task<GenericResponse> Remove(Guid id);

        Task<GenericResponse> MoveToWarehose(Receipt receipt);
        Task<GenericResponse> RetriveFromWarehose(Receipt receipt);

        Task<GenericResponse> AddDetail(ReceiptDetail detail);
        Task<GenericResponse> UpdateDetail(ReceiptDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);
        Task<GenericResponse> CalculateDetailWeightAndPrice(ReceiptDetail detail);
    }
}