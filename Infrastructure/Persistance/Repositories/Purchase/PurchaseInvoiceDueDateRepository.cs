using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceDueDateRepository : Repository<PurchaseInvoiceDueDate, Guid>, IPurchaseInvoiceDueDateRepository
    {
        public PurchaseInvoiceDueDateRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
