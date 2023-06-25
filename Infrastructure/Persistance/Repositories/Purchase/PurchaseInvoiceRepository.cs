using Application.Persistance.Repositories.Purchase;
using Domain.Entities.Purchase;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Purchase
{
    public class PurchaseInvoiceRepository : Repository<PurchaseInvoice, Guid>, IPurchaseInvoiceRepository
    {
        public PurchaseInvoiceRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<PurchaseInvoice?> Get(Guid id)
        {
            return await dbSet.Include(d => d.PurchaseInvoiceDueDates).AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }

        public override IEnumerable<PurchaseInvoice> Find(Expression<Func<PurchaseInvoice, bool>> predicate)
        {
            return dbSet
                .Include(d => d.PurchaseInvoiceDueDates).AsNoTracking()
                .Where(predicate)
                .OrderBy(pi => pi.PurchaseInvoiceDate);
        }

        public int GetNextNumber()
        {
            if (dbSet.Count() == 0) return 1;
            else return dbSet.Max(x => x.Number) + 1;
        }
    }
}
