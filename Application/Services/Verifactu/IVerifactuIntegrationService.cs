using Application.Contract;
using Application.Contracts;
using Domain.Entities.Sales;

namespace Application.Services.Sales;

public interface IVerifactuIntegrationService
{
    Task<GenericResponse> FindInvoicesInVerifactu(int month, int year);
    Task<GenericResponse> SendInvoiceToVerifactu(Guid id);
    Task<GenericResponse> RemoveInvoiceFromVerifactu(Guid id);
    Task<IEnumerable<SalesInvoice>> GetInvoicesToIntegrateWithVerifactu();
    Task<bool> HasInvoiceBeenIntegrated(Guid id);
    Task<IEnumerable<SalesInvoiceVerifactuRequest>> GetInvoiceRequests(Guid id);
    SalesInvoiceVerifactuRequest? GetLastSuccessfullRequest();
    Task<GenericResponse> CreateInvoiceRequest(SalesInvoiceVerifactuRequest verifactuRequest);
}
