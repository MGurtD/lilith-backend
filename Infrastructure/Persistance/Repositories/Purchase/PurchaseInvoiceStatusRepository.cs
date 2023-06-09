using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceStatusRepository : Repository<PurchaseInvoiceStatus, Guid>, IPurchaseInvoiceStatusRepository
    {
        public IPurchaseInvoiceStatusTransitionRepository TransitionRepository { get; }

        public PurchaseInvoiceStatusRepository(ApplicationDbContext context, IPurchaseInvoiceStatusTransitionRepository purchaseInvoiceStatusTransitionRepository) : base(context)
        {
            TransitionRepository = purchaseInvoiceStatusTransitionRepository;
        }   

        public async Task AddTransition(PurchaseInvoiceStatusTransition transition)
        {
            await TransitionRepository.Add(transition);
        }

        public PurchaseInvoiceStatusTransition? GetTransationById(Guid id)
        {
            var searchTask = TransitionRepository.Get(id);
            return searchTask.Result;
        }

        public async Task RemoveTransition(PurchaseInvoiceStatusTransition transtion)
        {
            await TransitionRepository.Remove(transtion);
        }

        public async Task UpdateTranstion(PurchaseInvoiceStatusTransition transtion)
        {
            await TransitionRepository.Update(transtion);
        }
    }
}
