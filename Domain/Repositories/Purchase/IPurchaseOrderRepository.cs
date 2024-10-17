using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase;

public interface IPurchaseOrderRepository : IRepository<PurchaseOrder, Guid>
{
    IRepository<PurchaseOrderDetail, Guid> Details { get; }
    IRepository<PurchaseOrderReceiptDetail, Guid> Receptions { get; }
    Task<PurchaseOrder> GetHeaders(Guid purchaseOrderId);
    Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id);
    Task<List<PurchaseOrder>> GetOrdersWithDetailsToReceiptBySupplier(Guid supplierId, List<Guid> discartedStatuses);
    Task<List<PurchaseOrderDetail>> GetOrderDetailsFromReceptions(List<PurchaseOrderReceiptDetail> receptions);
}
