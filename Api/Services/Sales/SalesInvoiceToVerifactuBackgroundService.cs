using Api;
using Application.Persistance;
using Application.Services.Sales;
using Domain.Entities.Sales;
using Microsoft.Extensions.Options;
using Verifactu;
using Verifactu.Contracts;

public class SalesInvoiceToVerifactuBackgroundService(IOptions<AppSettings> settings, IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Configura el intervalo deseado

            using (var scope = scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var enterprise = (await unitOfWork.Enterprises.FindAsync(e => !e.Disabled)).FirstOrDefault();

                var invoiceService = scope.ServiceProvider.GetRequiredService<ISalesInvoiceService>();
                var verifactuSettings = settings.Value.Verifactu!;
                var verifactuInvoiceService = new VerifactuInvoiceService(verifactuSettings.Url, verifactuSettings.Certificate.Path, verifactuSettings.Certificate.Password);
                var lastHash = invoiceService.GetLastHashSentToVerifactu();

                // Consultem les factures pendents d'enviament
                var pendingInvoices = await invoiceService.GetPendingInvoicesToSendToVerifactu();
                foreach (var invoice in pendingInvoices)
                {
                    var request = new RegisterInvoiceRequest
                    {
                        Enterprise = enterprise!,
                        SalesInvoice = invoice,
                        PreviousHash = lastHash
                    };

                    var response = await verifactuInvoiceService.RegisterInvoice(request);
                    var verifactuRequest = new SalesInvoiceVerifactuRequest
                    {
                        Hash = response.Hash,
                        Request = response.XmlRequest,
                        Response = response.XmlResponse,
                        SalesInvoiceId = invoice.Id,
                        Status = response.Success
                    };
                    await invoiceService.CreateVerifactuRequest(verifactuRequest);

                    lastHash = response.Hash; // Actualitzem l'últim hash enviat a Verifactu
                }

            }
        }
    }
}
