using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Services.Sales;

public interface ISalesInvoiceToVerifactuService
{
    Task<IEnumerable<SalesInvoice>> GetPendingSalesInvoicesToSend(Guid statusId);
    Task<GenericResponse> SendSalesInvoiceToVerifactu(SalesInvoice salesInvoice);
    Task<GenericResponse> CreateVerifactuRequest(SalesInvoiceVerifactuRequest verifactuRequest);
}
