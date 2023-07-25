using Application.Contracts.Purchase;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Infrastructure.Persistance
{
    public static class ApplicationDbContextConstants
    {
        public const int DECIMAL_PRECISION = 18;
        public const int DECIMAL_SCALE = 4;
    }
    public static class ApplicationDbContextSchemas
    {
        public const string CONFIG = "Config";
        public const string ACCOUNTABILITY = "Accounting";
    }

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder
                .Entity<ConsolidatedExpense>()
                .ToView("vw_consolidatedExpenses")
                .HasNoKey();

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

    }
}
