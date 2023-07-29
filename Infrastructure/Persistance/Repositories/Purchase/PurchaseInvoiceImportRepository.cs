using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceImportRepository : Repository<PurchaseInvoiceImport, Guid>, IPurchaseInvoiceImportRepository
    {
        public PurchaseInvoiceImportRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
