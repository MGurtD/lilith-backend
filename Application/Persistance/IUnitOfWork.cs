using Application.Contracts.Production;
using Application.Contracts.Purchase;
using Application.Contracts.Sales;
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
using Domain.Repositories.Sales;

namespace Application.Persistance
{
    public interface IUnitOfWork
    {
        // Authentication
        IRepository<Role, Guid> Roles { get; }
        IRepository<User, Guid> Users { get; }
        IRepository<UserRefreshToken, Guid> UserRefreshTokens { get; }
        IRepository<UserFilter, Guid> UserFilters { get; }
        IRepository<Profile, Guid> Profiles { get; }
        IRepository<MenuItem, Guid> MenuItems { get; }
        IRepository<ProfileMenuItem, Guid> ProfileMenuItems { get; }

        // Shared
        IRepository<Domain.Entities.File, Guid> Files { get; }
        IRepository<Parameter, Guid> Parameters { get; }
        IRepository<Exercise, Guid> Exercices { get; }
        IRepository<Tax, Guid> Taxes { get; }
        IRepository<PaymentMethod, Guid> PaymentMethods { get; }
        ILifecycleRepository Lifecycles { get; }
        // Removed Languages repository (now JSON based catalog)

        // Purchase
        IRepository<SupplierType, Guid> SupplierTypes { get; }
        ISupplierRepository Suppliers { get; }
        IPurchaseInvoiceStatusRepository PurchaseInvoiceStatuses { get; }
        IPurchaseOrderRepository PurchaseOrders { get; }
        IPurchaseInvoiceRepository PurchaseInvoices { get; }
        IRepository<PurchaseInvoiceDueDate, Guid> PurchaseInvoiceDueDates { get; }
        IRepository<InvoiceSerie, Guid> InvoiceSeries { get; }
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
        IRepository<SalesInvoiceVerifactuRequest, Guid> VerifactuRequests { get; }
        IDeliveryNoteRepository DeliveryNotes { get; }
        IBudgetRepository Budgets { get; }
        IContractReader<ConsolidatedIncomes> ConsolidatedIncomes { get; }

        // Production
        IRepository<Enterprise, Guid> Enterprises { get; }
        IRepository<Site, Guid> Sites { get; }
        IAreaRepository Areas { get; }
        IRepository<WorkcenterType, Guid> WorkcenterTypes { get; }
        IWorkcenterRepository Workcenters { get; }
        IRepository<WorkcenterCost, Guid> WorkcenterCosts { get; }
        IRepository<Operator, Guid> Operators { get;  }
        IRepository<OperatorType, Guid> OperatorTypes { get; }        
        IMachineStatusRepository MachineStatuses { get; }
        IRepository<Shift, Guid> Shifts { get; }
        IRepository<ShiftDetail, Guid> ShiftDetails { get; }
        IWorkMasterRepository WorkMasters { get; }
        IWorkOrderRepository WorkOrders { get; }
        IProductionPartRepository ProductionParts { get; }
        IWorkcenterShiftRepository WorkcenterShifts { get; }
        IContractReader<DetailedWorkOrder> DetailedWorkOrders { get; }
        IContractReader<ProductionCost> ProductionCosts { get; }
        IContractReader<WorkcenterShiftHistoricalOperator> WorkcenterShiftHistoricalOperators { get; }

        //Warehouse
        IWarehouseRepository Warehouses { get; }
        IRepository<ReferenceType, Guid> ReferenceTypes { get; }
        IRepository<Stock, Guid> Stocks { get; }
        IRepository<StockMovement, Guid> StockMovements {get; }

        Task<int> CompleteAsync();
        void Dispose();
    }
}
