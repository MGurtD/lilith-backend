using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceStatusRepository : Repository<PurchaseInvoiceStatus, Guid>, IPurchaseInvoiceStatusRepository
    {
        public IPurchaseInvoiceStatusTransitionRepository TransitionRepository { get; }

        public PurchaseInvoiceStatusRepository(ApplicationDbContext context, IPurchaseInvoiceStatusTransitionRepository purchaseInvoiceStatusTransitionRepository) : base(context)
        {
            TransitionRepository = purchaseInvoiceStatusTransitionRepository;
        }

        public override async Task<PurchaseInvoiceStatus?> Get(Guid id)
        {
            var transitions = TransitionRepository.GetTransitionsFromStatusFromId(id);
            var status = await dbSet.FindAsync(id);

            if (status != null)
            {
                if (transitions != null)
                {
                    status.Transitions = new List<PurchaseInvoiceStatus>();
                    foreach (var transition in transitions.ToList())
                    {
                        status.Transitions.Add(transition.ToStatus);
                    }
                }
            }

            return status;
        }

        public async Task AddTransition(PurchaseInvoiceStatusTransition transition)
        {
            await TransitionRepository.Add(transition);
        }

        public PurchaseInvoiceStatusTransition? GetTransitionById(Guid id)
        {
            var searchTask = TransitionRepository.Get(id);
            return searchTask.Result;
        }

        public async Task RemoveTransition(PurchaseInvoiceStatusTransition transition)
        {
            await TransitionRepository.Remove(transition);
        }

        public async Task UpdateTransition(PurchaseInvoiceStatusTransition transition)
        {
            await TransitionRepository.Update(transition);
        }
    }
}
