using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IReceiptRepository : IRepository<Receipt, Guid>
    {
        IRepository<ReceiptDetail, Guid> Details { get; }
        IRepository<PurchaseOrderReceiptDetail, Guid> Receptions { get; }

        Task<List<PurchaseOrderReceiptDetail>> GetReceptions(Guid id);
    }
}
