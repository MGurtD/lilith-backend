using Api.Services;
using Api.Services.Production;
using Api.Services.Purchase;
using Api.Services.Sales;
using Api.Services.Verifactu;
using Api.Services.Warehouse;
using Application.Persistance;
using Application.Production.Warehouse;
using Application.Services;
using Application.Services.Production;
using Application.Services.Purchase;
using Application.Services.Sales;
using Application.Services.Warehouse;
using Infrastructure.Persistance;
using Verifactu;

namespace Api.Setup;

public static class ApplicationServicesSetup
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IDueDateService, DueDateService>();
        services.AddScoped<IReferenceService, ReferenceService>();
        services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<ISalesOrderService, SalesOrderService>();
        services.AddScoped<IDeliveryNoteService, DeliveryNoteService>();
        services.AddScoped<ISalesInvoiceService, SalesInvoiceService>();
        services.AddScoped<ISalesInvoiceReportService, SalesInvoiceReportService>();
        services.AddScoped<ISalesOrderReportService, SalesOrderReportService>();
        services.AddScoped<IBudgetReportService, BudgetReportService>();
        services.AddScoped<IDeliveryNoteReportService, DeliveryNoteReportService>();
        services.AddScoped<IPurchaseOrderReportService, PurchaseOrderReportService>();
        services.AddScoped<ISalesOrderReportService, SalesOrderReportService>();
        services.AddScoped<IEnterpriseService, EnterpriseService>();
        services.AddScoped<IWorkOrderService, WorkOrderService>();
        services.AddScoped<IMetricsService, MetricsService>();
        services.AddScoped<IWorkcenterShiftService, WorkcenterShiftService>();
        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockMovementService, StockMovementService>();
        services.AddScoped<IVerifactuIntegrationService, VerifactuIntegrationService>();
        services.AddScoped<IProfileService, ProfileService>();
    services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddSingleton<IQrCodeService, QrCodeService>();

        services.AddHostedService<BudgetBackgroundService>();

        return services;
    }
}
