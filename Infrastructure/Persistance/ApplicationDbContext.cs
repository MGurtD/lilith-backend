using Domain.Entities;
using Infrastructure.Persistance.EntityConfiguration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Operator> Operators => Set<Operator>();
        public DbSet<Customer> Customers => Set<Customer>();

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            new OperatorBuilder().Configure(builder.Entity<Operator>());
            new CustomerBuilder().Configure(builder.Entity<Customer>());
        }

    }
}
