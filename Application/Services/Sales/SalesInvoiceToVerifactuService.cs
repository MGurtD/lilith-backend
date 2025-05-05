using Application.Contract;
using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Domain.Entities.Sales;

namespace Application.Services.Sales;

public class SalesInvoiceToVerifactuService(IUnitOfWork unitOfWork) : ISalesInvoiceToVerifactuService
{

    public async Task<IEnumerable<SalesInvoice>> GetPendingSalesInvoicesToSend(Guid statusId)
    {
        var salesInvoiceLifeCycle = await unitOfWork.Lifecycles.GetByName("SalesInvoice");
        if (salesInvoiceLifeCycle == null)
            throw new Exception("SalesInvoice lifecycle not found");

        if (salesInvoiceLifeCycle.FinalStatusId == null)
            throw new Exception("SalesInvoice lifecycle does not have a final status");

        return unitOfWork.SalesInvoices.GetPendingVerifactu(salesInvoiceLifeCycle.FinalStatusId.Value);
    }

    public async Task<GenericResponse> SendSalesInvoiceToVerifactu(SalesInvoice salesInvoice)
    {
        return new GenericResponse(true, salesInvoice);
    }

    public async Task<GenericResponse> CreateVerifactuRequest(SalesInvoiceVerifactuRequest verifactuRequest)
    {
        await unitOfWork.VerifactuRequests.Add(verifactuRequest);
        return new GenericResponse(true, verifactuRequest);
    }
}
