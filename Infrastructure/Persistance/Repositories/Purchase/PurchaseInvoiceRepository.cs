using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceRepository : Repository<PurchaseInvoice, Guid>, IPurchaseInvoiceRepository
    {
        public PurchaseInvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
