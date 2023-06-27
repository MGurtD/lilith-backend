using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Auth;
using Application.Persistance.Repositories.Production;
using Application.Persistance.Repositories.Purchase;
using Application.Persistance.Repositories.Sales;

namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        IRoleRepository Roles { get; }
        IUserRepository Users { get; }
        IUserRefreshTokenRepository UserRefreshTokens { get; }
        IFileRepository Files { get; }

        IExerciseRepository Exercices { get; }
        ITaxRepository Taxes { get; }
        IPaymentMethodRepository PaymentMethods { get; }

        ISupplierTypeRepository SupplierTypes { get; }
        ISupplierRepository Suppliers { get; }
        IPurchaseInvoiceRepository PurchaseInvoices { get; }
        IPurchaseInvoiceDueDateRepository PurchaseInvoiceDueDates { get; }
        IPurchaseInvoiceSerieRepository PurchaseInvoiceSeries { get; }
        IPurchaseInvoiceStatusRepository PurchaseInvoiceStatuses { get; }


        ICustomerTypeRepository CustomerTypes { get; }
        ICustomerRepository Customers { get; }

        IEnterpriseRepository Enterprises { get; }
        ISiteRepository Sites { get; }
        IWorkcenterRepository Workcenters { get; }
        IAreaRepository Areas { get; }
        IWorkcenterTypeRepository WorkcenterTypes { get; }


        Task<int> CompleteAsync();
    }
}
