using Application.Contracts;
using Application.Contracts.Purchase;
using Domain.Entities.Purchase;

namespace Application.Services.Purchase
{
    public interface IPurchaseOrderService
    {
        Task<PurchaseOrderReportResponse?> GetDtoForReportingById(Guid id);
        Task<List<PurchaseOrder>> GetBetweenDates(DateTime startDate, DateTime endDate, Guid? supplier, Guid? statusId);
        Task<List<PurchaseOrder>> GetBySupplier(Guid supplierId);

        Task<GenericResponse> Create(CreatePurchaseDocumentRequest createRequest);
        Task<GenericResponse> CreateFromWo(PurchaseOrderFromWO[] request);
        Task<GenericResponse> Update(PurchaseOrder prder);
        Task<GenericResponse> Remove(Guid id);
        Task<GenericResponse> DeterminateStatus(Guid id);

        Task<GenericResponse> AddDetail(PurchaseOrderDetail detail);
        Task<GenericResponse> UpdateDetail(PurchaseOrderDetail detail);
        Task<GenericResponse> RemoveDetail(Guid id);
        Task<GenericResponse> AddReceivedQuantityAndCalculateStatus(PurchaseOrderDetail detail, int quantity);
        Task<GenericResponse> SubstractReceivedQuantityAndCalculateStatus(PurchaseOrderDetail detail, int quantity);

        Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id);
        Task<List<PurchaseOrder>> GetOrdersWithDetailsToReceiptBySupplier(Guid supplierId);
        Task<GenericResponse> AddReception(PurchaseOrderReceiptDetail reception);
        Task<GenericResponse> RemoveReception(PurchaseOrderReceiptDetail reception);
    }
}