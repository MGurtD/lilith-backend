using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IReceiptRepository : IRepository<Receipt, Guid>
    {
        IRepository<ReceiptDetail, Guid> Details { get; }
    }
}
