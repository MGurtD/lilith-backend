using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IPurchaseInvoiceStatusRepository : IRepository<PurchaseInvoiceStatus, Guid>
    {
        IPurchaseInvoiceStatusTransitionRepository TransitionRepository { get; }
        PurchaseInvoiceStatusTransition? GetTransitionById(Guid id);
        Task AddTransition(PurchaseInvoiceStatusTransition transition);
        Task UpdateTransition(PurchaseInvoiceStatusTransition transtion);
        Task RemoveTransition(PurchaseInvoiceStatusTransition transtion);

    }
}
