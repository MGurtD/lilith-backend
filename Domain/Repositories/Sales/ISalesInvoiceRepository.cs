using Application.Persistance.Repositories;
using Domain.Entities.Sales;

namespace Domain.Repositories.Sales
{
    public interface ISalesInvoiceRepository : IRepository<SalesInvoice, Guid>
    {
        IRepository<SalesInvoiceDetail, Guid> InvoiceDetails { get; }
        IRepository<SalesInvoiceImport, Guid> InvoiceImports { get; }
        IRepository<SalesInvoiceDueDate, Guid> InvoiceDueDates { get; }
        Task<SalesInvoice?> GetHeader(Guid id);
        IEnumerable<SalesInvoice> GetPendingVerifactu(Guid statusId);

    }
}

