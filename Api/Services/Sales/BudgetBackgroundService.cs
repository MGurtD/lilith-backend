using Application.Services.Sales;

public class BudgetBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BudgetBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var budgetService = scope.ServiceProvider.GetRequiredService<IBudgetService>();

                // Llamada a la función de IBudgetService que necesitas
                await budgetService.RejectOutdatedBudgets();
            }

            await Task.Delay(TimeSpan.FromHours(8), stoppingToken); // Configura el intervalo deseado
        }
    }
}
