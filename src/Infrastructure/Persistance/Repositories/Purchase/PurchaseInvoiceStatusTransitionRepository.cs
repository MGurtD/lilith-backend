using Application.Contracts;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceStatusTransitionRepository : Repository<PurchaseInvoiceStatusTransition, Guid>, IPurchaseInvoiceStatusTransitionRepository
    {
        public PurchaseInvoiceStatusTransitionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IEnumerable<PurchaseInvoiceStatusTransition> GetTransitionsFromStatusFromId(Guid statusId)
        {
            var transitions = dbSet.Include(s => s.ToStatus).Where(t => t.FromStatusId == statusId);
            return transitions;
        }
    }
}
