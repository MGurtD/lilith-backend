using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceStatusTransitionRepository : Repository<PurchaseInvoiceStatusTransition, Guid>, IPurchaseInvoiceStatusTransitionRepository
    {
        public PurchaseInvoiceStatusTransitionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
