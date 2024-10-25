using Domain.Entities.Sales;

namespace Application.Persistance.Repositories.Sales
{
    public interface ISalesInvoiceRepository : IRepository<SalesInvoice, Guid>
    {
        Task<SalesInvoice?> GetHeader(Guid id);
        IRepository<SalesInvoiceDetail, Guid> InvoiceDetails { get; }
        IRepository<SalesInvoiceImport, Guid> InvoiceImports { get; }
        IRepository<SalesInvoiceDueDate, Guid> InvoiceDueDates { get; }        

    }
}

