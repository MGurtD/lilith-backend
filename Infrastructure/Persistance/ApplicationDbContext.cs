using Application.Contracts.Production;
using Application.Contracts.Purchase;
using Application.Contracts.Sales;
using Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance
{
    public static class ApplicationDbContextConstants
    {
        public const int DECIMAL_PRECISION = 18;
        public const int DECIMAL_SCALE = 4;
        public const int PRICE_DECIMAL_SCALE = 2;
        public const string TIMESTAMP_WITHOUT_TIMEZONE = "timestamp without time zone";
    }
    public static class ApplicationDbContextSchemas
    {
        public const string PUBLIC = "public";
        public const string DATA = "data";
    }

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<HttpTransactionLog>? HttpTransactionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder
                .Entity<ConsolidatedExpense>()
                .ToView("vw_consolidatedExpenses")
                .HasNoKey();

            builder
               .Entity<SalesInvoiceDetailReport>()
               .ToView("vw_report_salesInvoiceDetails")
               .HasNoKey();
            builder
                .Entity<DetailedWorkOrder>()
                .ToView("vw_detailedworkorder")
                .HasNoKey();
            builder
                .Entity<ProductionCost>()
                .ToView("vw_productioncosts")
                .HasNoKey();
            builder
                .Entity<ConsolidatedIncomes>()
                .ToView("vw_consolidatedIncomes")
                .HasNoKey();
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

    }
}
