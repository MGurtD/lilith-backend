using Application.Persistance;
using Application.Services.Sales;
using Domain.Entities.Sales;
using Verifactu;
using Verifactu.Contracts;

public class SalesInvoiceToVerifactuBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var enterprise = (await unitOfWork.Enterprises.FindAsync(e => !e.Disabled)).FirstOrDefault();

                var invoiceService = scope.ServiceProvider.GetRequiredService<ISalesInvoiceService>();
                var verifactuInvoiceService = scope.ServiceProvider.GetRequiredService<IVerifactuInvoiceService>();
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

            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Configura el intervalo deseado
        }
    }
}
