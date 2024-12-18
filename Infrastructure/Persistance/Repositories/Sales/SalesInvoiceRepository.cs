﻿using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Sales;
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
                        .FirstOrDefaultAsync(e => e.Id == id);
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