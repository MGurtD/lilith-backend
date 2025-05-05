using Application.Services.Sales;
using Verifactu;

public class SalesInvoiceToVerifactuBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var invoiceService = scope.ServiceProvider.GetRequiredService<ISalesInvoiceService>();
                var verifactuInvoiceService = scope.ServiceProvider.GetRequiredService<IVerifactuInvoiceService>();

                // Llamada a la función de IBudgetService que necesitas
                
            }

            await Task.Delay(TimeSpan.FromHours(8), stoppingToken); // Configura el intervalo deseado
        }
    }
}
