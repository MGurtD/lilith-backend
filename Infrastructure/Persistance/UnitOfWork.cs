using Application.Contracts.Production;
using Application.Contracts.Purchase;
using Application.Persistance;
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
using Infrastructure.Persistance.Repositories;
using Infrastructure.Persistance.Repositories.Production;
using Infrastructure.Persistance.Repositories.Purchase;
using Infrastructure.Persistance.Repositories.Sales;
using Infrastructure.Persistance.Repositories.Warehouse;

namespace Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public readonly ApplicationDbContext context;

        // Authentication
        public IRepository<Role, Guid> Roles { get; private set; }
        public IRepository<User, Guid> Users { get; private set; }
        public IRepository<UserRefreshToken, Guid> UserRefreshTokens { get; private set; }

        // Shared
        public IRepository<Domain.Entities.File, Guid> Files { get; private set; }
        public IRepository<Parameter, Guid> Parameters { get; private set; }
        public IRepository<Exercise, Guid> Exercices { get; private set; }
        public IRepository<Tax, Guid> Taxes { get; private set; }
        public IRepository<PaymentMethod, Guid> PaymentMethods { get; private set; }
        public ILifecycleRepository Lifecycles { get; private set; }

        // Purchase
        public IRepository<SupplierType, Guid> SupplierTypes { get; private set; }
        public ISupplierRepository Suppliers { get; private set; }
        public IPurchaseInvoiceStatusRepository PurchaseInvoiceStatuses { get; private set; }
        public IPurchaseInvoiceRepository PurchaseInvoices { get; private set; }
        public IRepository<PurchaseInvoiceDueDate, Guid> PurchaseInvoiceDueDates { get; private set; }
        public IRepository<PurchaseInvoiceSerie, Guid> PurchaseInvoiceSeries { get; private set; }
        public IRepository<ExpenseType, Guid> ExpenseTypes { get; private set; }
        public IExpenseRepository Expenses { get; private set; }
        public IRepository<ReferenceFormat, Guid> ReferenceFormats { get; private set; }
        public IContractReader<ConsolidatedExpense> ConsolidatedExpenses { get; private set; }        

        // Sales
        public IRepository<CustomerType, Guid> CustomerTypes { get; private set; }
        public ICustomerRepository Customers { get; private set; }
        public IRepository<Reference, Guid> References { get; private set; }
        public ISalesOrderHeaderRepository SalesOrderHeaders { get; private set; }
        public ISalesOrderDetailRepository SalesOrderDetails { get; private set; }
        public ISalesInvoiceRepository SalesInvoices { get; private set; }
        public IReceiptRepository Receipts { get; private set; }
        public IDeliveryNoteRepository DeliveryNotes { get; private set; }
        public IBudgetRepository Budgets { get; private set; }

        // Production
        public IRepository<Enterprise, Guid> Enterprises { get; private set; }
        public IRepository<Site, Guid> Sites { get; private set; }
        public IRepository<Area, Guid> Areas { get; private set; }
        public IRepository<WorkcenterType, Guid> WorkcenterTypes { get; private set; }
        public IWorkcenterRepository Workcenters { get; private set; }
        public IRepository<WorkCenterCost, Guid> WorkcenterCosts { get; private set; }
        public IRepository<Operator, Guid> Operators { get; private set; }
        public IRepository<OperatorType, Guid> OperatorTypes { get; private set; }        
        public IRepository<MachineStatus, Guid> MachineStatuses { get; private set; }
        public IRepository<Shift, Guid> Shifts { get; private set; }
        public IWorkMasterRepository WorkMasters { get; private set; }
        public IWorkOrderRepository WorkOrders { get; private set; }
        public IProductionPartRepository ProductionParts { get; private set; }
        public IContractReader<DetailedWorkOrder> DetailedWorkOrders { get; private set; }

        // Warehouse
        public IWarehouseRepository Warehouses { get; private set; }
        public IRepository<ReferenceType, Guid> ReferenceTypes { get; private set; }
        public IRepository<Stock, Guid> Stocks{ get; private set;}
        public IRepository<StockMovement, Guid> StockMovements {get; private set;}

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;

            Roles = new Repository<Role, Guid>(context);
            Users = new Repository<User, Guid>(context);
            UserRefreshTokens = new Repository<UserRefreshToken, Guid>(context);

            Files = new Repository<Domain.Entities.File, Guid>(context);    
            Parameters = new Repository<Parameter, Guid>(context);
            PaymentMethods = new Repository<PaymentMethod, Guid>(context);
            Taxes = new Repository<Tax, Guid>(context);
            Exercices = new Repository<Exercise, Guid>(context);

            SupplierTypes = new Repository<SupplierType, Guid>(context);
            Suppliers = new SupplierRepository(context);
            PurchaseInvoiceStatuses = new PurchaseInvoiceStatusRepository(context);
            PurchaseInvoices = new PurchaseInvoiceRepository(context);
            PurchaseInvoiceDueDates = new Repository<PurchaseInvoiceDueDate, Guid>(context);
            PurchaseInvoiceSeries = new Repository<PurchaseInvoiceSerie, Guid>(context);
            Receipts = new ReceiptRepository(context);
            ReferenceFormats = new Repository<ReferenceFormat, Guid>(context);
            ExpenseTypes = new Repository<ExpenseType, Guid>(context);
            Expenses = new ExpenseRepository(context);
            ConsolidatedExpenses = new ContractReader<ConsolidatedExpense>(context);

            CustomerTypes = new Repository<CustomerType, Guid>(context);
            Customers = new CustomerRepository(context);
            Lifecycles = new LifecycleRepository(context);
            References = new Repository<Reference, Guid>(context);
            SalesOrderHeaders = new SalesOrderHeaderRepository(context, new SalesOrderDetailRepository(context));
            SalesOrderDetails = new SalesOrderDetailRepository(context);
            SalesInvoices = new SalesInvoiceRepository(context);
            DeliveryNotes = new DeliveryNoteRepository(context);
            Budgets = new BudgetRepository(context);

            Enterprises = new Repository<Enterprise, Guid>(context);
            Sites = new Repository<Site, Guid>(context);
            Areas = new Repository<Area, Guid>(context);
            WorkcenterTypes = new Repository<WorkcenterType, Guid>(context);
            Workcenters = new WorkcenterRepository(context);
            WorkcenterCosts = new Repository<WorkCenterCost, Guid>(context);
            Operators = new Repository<Operator, Guid>(context);
            OperatorTypes = new Repository<OperatorType, Guid>(context);            
            MachineStatuses = new Repository<MachineStatus, Guid>(context);
            Shifts = new Repository<Shift, Guid>(context);
            WorkMasters = new WorkMasterRepository(context);
            WorkOrders = new WorkOrderRepository(context);
            ProductionParts = new ProductionPartRepository(context);
            DetailedWorkOrders = new ContractReader<DetailedWorkOrder>(context);

            Warehouses = new WarehouseRepository(context);
            ReferenceTypes = new Repository<ReferenceType, Guid>(context);
            Stocks = new Repository<Stock, Guid>(context);
            StockMovements = new Repository<StockMovement, Guid>(context);
        }

        public async Task<int> CompleteAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose() => context.Dispose();
    }
}

