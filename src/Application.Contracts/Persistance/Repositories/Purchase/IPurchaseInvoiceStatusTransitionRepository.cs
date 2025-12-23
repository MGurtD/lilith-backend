using Domain.Entities.Purchase;

namespace Application.Contracts
{
    public interface IPurchaseInvoiceStatusTransitionRepository : IRepository<PurchaseInvoiceStatusTransition, Guid>
    {

        IEnumerable<PurchaseInvoiceStatusTransition> GetTransitionsFromStatusFromId(Guid statusId);

    }
}
