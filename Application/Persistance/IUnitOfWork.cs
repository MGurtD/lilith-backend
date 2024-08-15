
using Application.Contracts.Production;
using Application.Contracts.Purchase;
using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Production;
using Application.Persistance.Repositories.Purchase;
using Application.Persistance.Repositories.Sales;
using Application.Persistance.Repositories.Warehouse;
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
        IRepository<UserFilter, Guid> UserFilters { get; }

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
        IPurchaseOrderRepository PurchaseOrders { get; }
        IPurchaseInvoiceRepository PurchaseInvoices { get; }
        IRepository<PurchaseInvoiceDueDate, Guid> PurchaseInvoiceDueDates { get; }
        IRepository<PurchaseInvoiceSerie, Guid> PurchaseInvoiceSeries { get; }
        IRepository<ExpenseType, Guid> ExpenseTypes { get; }
        IExpenseRepository Expenses { get; }
        IReceiptRepository Receipts { get; }
        IRepository<ReferenceFormat, Guid> ReferenceFormats { get; }
        IContractReader<ConsolidatedExpense> ConsolidatedExpenses { get; }

        // Sales
        IRepository<CustomerType, Guid> CustomerTypes { get; }
        ICustomerRepository Customers { get; }
        IRepository<Reference, Guid> References { get; }
        ISalesOrderHeaderRepository SalesOrderHeaders { get; }
        ISalesOrderDetailRepository SalesOrderDetails { get; }
        ISalesInvoiceRepository SalesInvoices { get; }
        IDeliveryNoteRepository DeliveryNotes { get; }
        IBudgetRepository Budgets { get; }

        // Production
        IRepository<Enterprise, Guid> Enterprises { get; }
        IRepository<Site, Guid> Sites { get; }
        IRepository<Area, Guid> Areas { get; }
        IRepository<WorkcenterType, Guid> WorkcenterTypes { get; }
        IWorkcenterRepository Workcenters { get; }
        IRepository<WorkCenterCost, Guid> WorkcenterCosts { get; }
        IRepository<Operator, Guid> Operators { get;  }
        IRepository<OperatorType, Guid> OperatorTypes { get; }        
        IRepository<MachineStatus, Guid> MachineStatuses { get; }
        IRepository<Shift, Guid> Shifts { get; }
        IRepository<ShiftDetail, Guid> ShiftDetails { get; }
        IWorkMasterRepository WorkMasters { get; }
        IWorkOrderRepository WorkOrders { get; }
        IProductionPartRepository ProductionParts { get; }
        IContractReader<DetailedWorkOrder> DetailedWorkOrders { get; }

        //Warehouse
        IWarehouseRepository Warehouses { get; }
        IRepository<ReferenceType, Guid> ReferenceTypes { get; }
        IRepository<Stock, Guid> Stocks { get; }
        IRepository<StockMovement, Guid> StockMovements {get; }

        Task<int> CompleteAsync();
        void Dispose();
    }
}
