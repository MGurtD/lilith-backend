using Domain.Entities.Purchase;

namespace Application.Persistance.Repositories.Purchase
{
    public interface IPurchaseInvoiceStatusRepository : IRepository<PurchaseInvoiceStatus, Guid>
    {
        IPurchaseInvoiceStatusTransitionRepository TransitionRepository { get; }
        PurchaseInvoiceStatusTransition? GetTransationById(Guid id);
        Task AddTransition(PurchaseInvoiceStatusTransition transition);
        Task UpdateTranstion(PurchaseInvoiceStatusTransition transtion);
        Task RemoveTransition(PurchaseInvoiceStatusTransition transtion);

    }
}
