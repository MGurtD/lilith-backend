using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceStatusRepository : Repository<PurchaseInvoiceStatus, Guid>, IPurchaseInvoiceStatusRepository
    {
        public PurchaseInvoiceStatusRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
