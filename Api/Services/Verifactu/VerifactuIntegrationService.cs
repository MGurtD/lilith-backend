using Application.Contracts;
using Application.Persistance;
using Application.Services.Sales;
using Domain.Entities.Sales;
using Microsoft.Extensions.Options;
using Verifactu.Contracts;
using Verifactu;

namespace Api.Services.Verifactu;
public class VerifactuIntegrationService(IUnitOfWork unitOfWork,
    ISalesInvoiceService salesInvoiceService,
    IOptions<AppSettings> settings) : IVerifactuIntegrationService
{
    private readonly AppSettings settings = settings.Value;
    private const string VerifactuLifecycleName = "Verifactu";

    public async Task<GenericResponse> SendInvoiceToVerifactu(Guid id)
    {
        var invoice = await unitOfWork.SalesInvoices.Get(id);
        if (invoice == null) return new GenericResponse(false, $"La factura amb ID {id} no existeix");
        if (invoice.SalesInvoiceDetails.Count == 0)
            return new GenericResponse(false, "La factura no té detalls. No es pot enviar a Verifactu");

        var enterprise = (await unitOfWork.Enterprises.FindAsync(e => !e.Disabled)).FirstOrDefault();
        if (enterprise == null)
            return new GenericResponse(false, "No s'ha trobat l'empresa per enviar la factura a Verifactu");

        // Recuperar l'última factura enviada per l'encadenament
        var lastSuccessfullRequest = GetLastSuccessfullRequest();
        var lastSuccessfullInvoice = lastSuccessfullRequest != null
            ? await salesInvoiceService.GetHeaderById(lastSuccessfullRequest.SalesInvoiceId)
            : null;

        // Construir la petició
        var request = new RegisterInvoiceRequest
        {
            Enterprise = enterprise,
            SalesInvoice = invoice,
            PreviousNotificatedInvoice = lastSuccessfullInvoice,
            PreviousHash = lastSuccessfullRequest?.Hash
        };

        // Enviar la factura a Verifactu
        var verifactuSettings = settings.Verifactu!;
        var verifactuService = new VerifactuInvoiceService(verifactuSettings.Url, verifactuSettings.Certificate.Path, verifactuSettings.Certificate.Password);
        var response = await verifactuService.RegisterInvoice(request);

        // Registrar la petición en la base de datos
        var verifactuRequest = new SalesInvoiceVerifactuRequest
        {
            Hash = response.Hash,
            Request = response.XmlRequest,
            Response = response.XmlResponse,
            SalesInvoiceId = invoice.Id,
            Success = response.Success,
            Status = response.StatusRegister,
            TimestampResponse = response.Timestamp
        };
        await CreateInvoiceRequest(verifactuRequest);

        // Actualitzar l'estat de la integració de la factura
        invoice.IntegrationStatusId = response.Success
            ? (await unitOfWork.Lifecycles.GetStatusByName(VerifactuLifecycleName, "OK"))?.Id
            : (await unitOfWork.Lifecycles.GetStatusByName(VerifactuLifecycleName, "Error"))?.Id;
        await unitOfWork.SalesInvoices.Update(invoice);

        return new GenericResponse(true, verifactuRequest);
    }

    public Task<GenericResponse> RemoveInvoiceFromVerifactu(Guid id)
    {
        throw new NotImplementedException("RemoveFromVerifactu is not implemented yet.");
    }

    public async Task<IEnumerable<SalesInvoice>> GetInvoicesToIntegrateWithVerifactu()
    {
        var salesInvoiceLifeCycle = await unitOfWork.Lifecycles.GetByName(VerifactuLifecycleName) ?? throw new Exception("El cicle de vida 'Verifactu' no existeix");
        if (salesInvoiceLifeCycle.InitialStatusId == null)
            throw new Exception("El cicle de vida no té un estat inicial");

        return unitOfWork.SalesInvoices.GetPendingToIntegrate(salesInvoiceLifeCycle.InitialStatusId.Value);
    }

    public async Task<IEnumerable<SalesInvoiceVerifactuRequest>> GetInvoiceRequests(Guid invoiceId)
    {
        return await unitOfWork.VerifactuRequests.FindAsync(i => i.SalesInvoiceId == invoiceId);
    }

    public async Task<bool> HasInvoiceBeenIntegrated(Guid id)
    {
        var requests = await GetInvoiceRequests(id);
        return requests.Any(r => r.Success);
    }

    public async Task<GenericResponse> CreateInvoiceRequest(SalesInvoiceVerifactuRequest verifactuRequest)
    {
        await unitOfWork.VerifactuRequests.Add(verifactuRequest);
        return new GenericResponse(true, verifactuRequest);
    }

    public SalesInvoiceVerifactuRequest? GetLastSuccessfullRequest()
    {
        var lastSuccessfullRequest = unitOfWork.VerifactuRequests.Find(vr => vr.Success == true)
            .OrderByDescending(vr => vr.CreatedOn)
            .FirstOrDefault();
        return lastSuccessfullRequest;
    }

    public async Task<GenericResponse> FindInvoicesInVerifactu(int month, int year)
    {
        var enterprise = (await unitOfWork.Enterprises.FindAsync(e => !e.Disabled)).FirstOrDefault();
        if (enterprise == null)
            return new GenericResponse(false, "No s'ha trobat l'empresa per enviar la factura");

        var site = (await unitOfWork.Sites.FindAsync(s => !s.Disabled)).FirstOrDefault();
        if (site == null)
            return new GenericResponse(false, "No s'ha trobat el lloc per enviar la factura");

        // Construir la petició
        var request = new FindInvoicesRequest
        {
            EnterpriseName = enterprise.Name,
            VatNumber = site.VatNumber,
            Month = month,
            Year = year,
        };

        // Enviar la factura a Verifactu
        var verifactuSettings = settings.Verifactu!;
        var verifactuInvoiceService = new VerifactuInvoiceService(verifactuSettings.Url, verifactuSettings.Certificate.Path, verifactuSettings.Certificate.Password);
        var response = await verifactuInvoiceService.FindInvoices(request);
        
        return new GenericResponse(true, response);
    }

}
