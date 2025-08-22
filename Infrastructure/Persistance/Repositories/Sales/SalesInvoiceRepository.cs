using Application.Persistance.Repositories;
using Domain.Entities.Sales;
using Domain.Repositories.Sales;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Persistance.Repositories.Sales
{
    public class SalesInvoiceRepository(ApplicationDbContext context) : Repository<SalesInvoice, Guid>(context), ISalesInvoiceRepository
    {
        public IRepository<SalesInvoiceDetail, Guid> InvoiceDetails { get; } = new Repository<SalesInvoiceDetail, Guid>(context);
        public IRepository<SalesInvoiceImport, Guid> InvoiceImports { get; } = new Repository<SalesInvoiceImport, Guid>(context);
        public IRepository<SalesInvoiceDueDate, Guid> InvoiceDueDates { get; } = new Repository<SalesInvoiceDueDate, Guid>(context);

        public override async Task<SalesInvoice?> Get(Guid id)
        {
            return await dbSet
                        .Include(s => s.Customer)
                        .Include(s => s.Site)
                        .Include(s => s.SalesInvoiceDetails)
                            .ThenInclude(d => d.DeliveryNoteDetail)
                        .Include(s => s.SalesInvoiceImports)
                            .ThenInclude(d => d.Tax)
                        .Include(s => s.SalesInvoiceDueDates)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<SalesInvoice?> GetHeader(Guid id)
        {
            return await dbSet
                        .AsNoTracking()
                        .SingleOrDefaultAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<SalesInvoiceImport>> GetImportsByInvoiceId(Guid id)
        {
            var imports = new List<SalesInvoiceImport>();
            var invoice = await dbSet
                        .Include(s => s.SalesInvoiceImports)
                            .ThenInclude(d => d.Tax)
                        .FirstOrDefaultAsync(e => e.Id == id);

            if (invoice is not null) imports.AddRange(invoice.SalesInvoiceImports);
            return imports;
        }

        public IEnumerable<SalesInvoice> GetPendingToIntegrate(Guid statusId)
        {
            return dbSet
                    .Include(s => s.Customer)
                    .Include(s => s.Site)
                    .Include(s => s.ParentSalesInvoice)
                    .Include(s => s.SalesInvoiceImports)
                        .ThenInclude(d => d.Tax)
                    .Include(s => s.SalesInvoiceDueDates)
                    .Include(s => s.VerifactuRequests)
                    .Where(e => e.IntegrationStatusId == statusId)
                    .OrderBy(e => e.InvoiceNumber);
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