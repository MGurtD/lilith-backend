using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Sales;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class SalesInvoiceRepository : Repository<SalesInvoice, Guid>, ISalesInvoiceRepository
    {
        public IRepository<SalesInvoiceDetail, Guid> InvoiceDetails { get; }
        public IRepository<SalesInvoiceImport, Guid> InvoiceImports { get; }
        public IRepository<SalesInvoiceDueDate, Guid> InvoiceDueDates { get; }

        public SalesInvoiceRepository(ApplicationDbContext context) : base(context)
        {
            InvoiceDetails = new Repository<SalesInvoiceDetail, Guid>(context);
            InvoiceImports = new Repository<SalesInvoiceImport, Guid>(context);
            InvoiceDueDates = new Repository<SalesInvoiceDueDate, Guid>(context);
        }

        public override async Task<SalesInvoice?> Get(Guid id)
        {
            return await dbSet
                        .Include("SalesInvoiceDetails.Reference")
                        .Include(s => s.SalesInvoiceImports)
                        .Include(s => s.SalesInvoiceDueDates)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public override IEnumerable<SalesInvoice> Find(Expression<Func<SalesInvoice, bool>> predicate)
        {
            return dbSet
                .AsNoTracking()
                .Include(d => d.SalesInvoiceDueDates)
                .Where(predicate)
                .OrderBy(pi => pi.InvoiceNumber);
        }

    }
}