using Application.Contracts.Sales;

namespace Application.Services.Sales;

public interface ISalesInvoiceReportService
{
    Task<InvoiceReportDto?> GetDtoForReportingById(Guid id);
}
