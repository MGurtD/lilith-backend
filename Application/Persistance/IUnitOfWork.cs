using Application.Contracts.Purchase;
using Application.Contracts.Sales;
using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Production;
using Application.Persistance.Repositories.Purchase;
using Application.Persistance.Repositories.Sales;
using Domain.Entities;
using Domain.Entities.Auth;
using Domain.Entities.Production;
using Domain.Entities.Purchase;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Domain.Entities.Warehouse;

namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        // Authentication
        IRepository<Role, Guid> Roles { get; }
        IRepository<User, Guid> Users { get; }
        IRepository<UserRefreshToken, Guid> UserRefreshTokens { get; }

        // Shared
        IRepository<Domain.Entities.File, Guid> Files { get; }
        IRepository<Parameter, Guid> Parameters { get; }
        IRepository<Exercise, Guid> Exercices { get; }
        IRepository<Tax, Guid> Taxes { get; }
        IRepository<PaymentMethod, Guid> PaymentMethods { get; }
        ILifecycleRepository Lifecycles { get; }

        // Purchase
        IRepository<SupplierType, Guid> SupplierTypes { get; }
        ISupplierRepository Suppliers { get; }
        IPurchaseInvoiceStatusRepository PurchaseInvoiceStatuses { get; }
        IPurchaseInvoiceRepository PurchaseInvoices { get; }
        IRepository<PurchaseInvoiceDueDate, Guid> PurchaseInvoiceDueDates { get; }
        IRepository<PurchaseInvoiceSerie, Guid> PurchaseInvoiceSeries { get; }
        IRepository<ExpenseType, Guid> ExpenseTypes { get; }
        IExpenseRepository Expenses { get; }
        IContractReader<ConsolidatedExpense> ConsolidatedExpenses { get; }

        // Sales
        IRepository<CustomerType, Guid> CustomerTypes { get; }
        ICustomerRepository Customers { get; }
        IRepository<Reference, Guid> References { get; }
        ISalesOrderHeaderRepository SalesOrderHeaders { get; }
        ISalesOrderDetailRepository SalesOrderDetails { get; }
        ISalesInvoiceRepository SalesInvoices { get; }
        IContractReader<Contracts.Sales.SalesOrderDetail> SalesOrderDetailForInvoices { get; }
        IContractReader<SalesInvoiceDetailReport> SalesInvoiceDetailWithOrder { get; }

        // Production
        IRepository<Enterprise, Guid> Enterprises { get; }
        IRepository<Site, Guid> Sites { get; }
        IRepository<Area, Guid> Areas { get; }
        IRepository<WorkcenterType, Guid> WorkcenterTypes { get; }
        IWorkcenterRepository Workcenters { get; }
        IRepository<WorkCenterCost, Guid> WorkcenterCosts { get; }
        IRepository<Operator, Guid> Operators { get;  }
        IRepository<OperatorType, Guid> OperatorTypes { get; }
        IRepository<OperatorCost, Guid> OperatorCosts { get; }
        IRepository<MachineStatus, Guid> MachineStatuses { get; }
        IRepository<Shift, Guid> Shifts { get; }

        //Warehouse
        IRepository<Warehouse, Guid> Warehouses { get; }
        IRepository<MaterialType, Guid> MaterialTypes { get; }
        IRepository<Location, Guid> Locations { get; }

        Task<int> CompleteAsync();
    }
}
