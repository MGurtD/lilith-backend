using Domain.Entities.Purchase;

namespace Application.Contracts
{
    public interface IPurchaseInvoiceStatusRepository : IRepository<PurchaseInvoiceStatus, Guid>
    {
        IPurchaseInvoiceStatusTransitionRepository TransitionRepository { get; }
        Task<PurchaseInvoiceStatusTransition?> GetTransitionById(Guid id);
        Task AddTransition(PurchaseInvoiceStatusTransition transition);
        Task UpdateTransition(PurchaseInvoiceStatusTransition transtion);
        Task<bool> RemoveTransition(Guid fromStatusId, Guid toStatusId);

    }
}
