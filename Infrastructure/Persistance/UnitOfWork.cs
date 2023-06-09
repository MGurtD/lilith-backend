﻿using Application.Persistance;
using Application.Persistance.Repositories;
using Application.Persistance.Repositories.Auth;
using Application.Persistance.Repositories.Expense;
using Application.Persistance.Repositories.Production;
using Application.Persistance.Repositories.Purchase;
using Application.Persistance.Repositories.Sales;
using Infrastructure.Persistance.Repositories;
using Infrastructure.Persistance.Repositories.Auth;
using Infrastructure.Persistance.Repositories.Expense;
using Infrastructure.Persistance.Repositories.Production;
using Infrastructure.Persistance.Repositories.Purchase;
using Infrastructure.Persistance.Repositories.Sales;

namespace Infrastructure.Persistance
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public readonly ApplicationDbContext context;
        public IEnterpriseRepository Enterprises { get; private set; }
        public ISiteRepository Sites { get; private set; }    
        public IAreaRepository Areas { get; private set; }
        public IWorkcenterRepository Workcenters { get; private set; }
        public IWorkcenterTypeRepository WorkcenterTypes { get; private set; }
        public IRoleRepository Roles { get; private set; }
        public IUserRepository Users { get; private set; }
        public IUserRefreshTokenRepository UserRefreshTokens { get; private set; }
        public IPaymentMethodRepository PaymentMethods { get; private set; }
        public IExerciseRepository Exercices { get; private set; }
        public ITaxRepository Taxes { get; private set; }
        public IFileRepository Files { get; private set; }
        public ISupplierTypeRepository SupplierTypes { get; private set; }
        public ISupplierRepository Suppliers { get; private set; }
        public IPurchaseInvoiceSerieRepository PurchaseInvoiceSeries { get; private set; }
        public IPurchaseInvoiceStatusRepository PurchaseInvoiceStatuses { get; private set; }
        public IPurchaseInvoiceRepository PurchaseInvoices { get; private set; }
        public IPurchaseInvoiceDueDateRepository PurchaseInvoiceDueDates { get; private set; }
        public ICustomerTypeRepository CustomerTypes { get; private set; }
        public ICustomerRepository Customers { get; private set; }
        public IExpenseTypeRepository ExpenseTypes { get; private set; }
        public IExpenseRepository Expenses { get; private set; }

        public IAreaRepository AreaRepositories => throw new NotImplementedException();

        public UnitOfWork(ApplicationDbContext context)
        {
            this.context = context;
            UserRefreshTokens = new UserRefreshTokenRepository(context);
            Users = new UserRepository(context);
            Roles = new RoleRepository(context);
            Files = new FileRepository(context);
            
            PaymentMethods = new PaymentMethodRepository(context);
            Taxes = new TaxRepository(context);
            Exercices = new ExerciseRepository(context);

            SupplierTypes = new SupplierTypeRepository(context);
            Suppliers = new SupplierRepository(context, new SupplierContactRepository(context));
            PurchaseInvoices = new PurchaseInvoiceRepository(context);
            PurchaseInvoiceDueDates = new PurchaseInvoiceDueDateRepository(context);
            PurchaseInvoiceSeries = new PurchaseInvoiceSerieRepository(context);
            PurchaseInvoiceDueDates = new PurchaseInvoiceDueDateRepository(context);
            PurchaseInvoiceStatuses = new PurchaseInvoiceStatusRepository(context, new PurchaseInvoiceStatusTransitionRepository(context));

            CustomerTypes = new CustomerTypeRepository(context);
            Customers = new CustomerRepository(context, new CustomerContactRepository(context), new CustomerAddressRepository(context));

            Enterprises = new EnterpriseRepository(context);
            Sites = new SiteRepository(context);
            Areas = new AreaRepository(context);
            Workcenters = new WorkcenterRepository(context);
            WorkcenterTypes = new WorkcenterTypeRepository(context);
            
            ExpenseTypes = new ExpenseTypeRepository(context);
            Expenses = new ExpenseRepository(context);

        }

        public async Task<int> CompleteAsync()
        {
            return await context.SaveChangesAsync();
        }

        public void Dispose()
        {
            context.Dispose();
        }
    }
}

