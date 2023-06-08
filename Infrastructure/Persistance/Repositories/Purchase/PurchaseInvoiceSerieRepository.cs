using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceSerieRepository : Repository<PurchaseInvoiceSerie, Guid>, IPurchaseInvoiceSerieRepository
    {
        public PurchaseInvoiceSerieRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
