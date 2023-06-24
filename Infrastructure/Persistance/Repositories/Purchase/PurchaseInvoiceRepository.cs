using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceRepository : Repository<PurchaseInvoice, Guid>, IPurchaseInvoiceRepository
    {
        public PurchaseInvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<PurchaseInvoice?> Get(Guid id)
        {
            return await dbSet.Include(s => s.Supplier).Include(s => s.PaymentMethod).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public int GetNextNumber()
        {
            return dbSet.Max(x => x.Number) + 1;
        }
    }
}
