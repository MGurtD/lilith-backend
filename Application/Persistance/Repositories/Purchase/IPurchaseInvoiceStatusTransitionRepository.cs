using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IPurchaseInvoiceStatusTransitionRepository : IRepository<PurchaseInvoiceStatusTransition, Guid>
    {
    }
}
