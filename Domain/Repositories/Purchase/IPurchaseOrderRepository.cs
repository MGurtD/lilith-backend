using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IPurchaseOrderRepository : IRepository<PurchaseOrder, Guid>
    {
        IRepository<PurchaseOrderDetail, Guid> Details { get; }
    }
}
