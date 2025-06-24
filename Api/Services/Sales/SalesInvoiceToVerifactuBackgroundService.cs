using Application.Persistance;
using Application.Services.Sales;
using Domain.Entities.Sales;

namespace Api.Services.Sales;

public class SalesInvoiceToVerifactuBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Configura el intervalo deseado
            
            using var scope = scopeFactory.CreateScope();
            var invoiceService = scope.ServiceProvider.GetRequiredService<ISalesInvoiceService>();

            // Consultem les factures pendents d'enviament
           /* var pendingInvoices = await invoiceService.GetInvoicesPendingIntegrationWithVerifactu();
            foreach (var invoice in pendingInvoices)
            {
                var response = await invoiceService.SendToVerifactu(invoice.Id);
                logger.LogInformation($"Enviament de la factura {invoice.InvoiceNumber} a Verifactu: {response.Result}");
                logger.LogInformation($"Petició: {((SalesInvoiceVerifactuRequest) response.Content!).Request}");
                logger.LogInformation($"Resposta: {((SalesInvoiceVerifactuRequest) response.Content!).Response}");
            }*/
        }
    }
}
