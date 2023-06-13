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

        public async Task<PurchaseInvoiceStatusTransition?> GetTransitionById(Guid id)
        {
            var searchTask = await TransitionRepository.Get(id);
            return searchTask;
        }

        public async Task UpdateTransition(PurchaseInvoiceStatusTransition transition)
        {
            await TransitionRepository.Update(transition);
        }

        public async Task<bool> RemoveTransition(Guid fromStatusId, Guid toStatusId)
        {
            var transition = TransitionRepository.Find(t => t.FromStatusId == fromStatusId && t.ToStatusId == toStatusId).FirstOrDefault();
            if (transition != null)
            {
                await TransitionRepository.Remove(transition);
                return true;
            }
            return false;
        }
    }
}
