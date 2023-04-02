using Domain.Entities;
using Infrastructure.Persistance.EntityConfiguration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            new CustomerBuilder().Configure(builder.Entity<Customer>());
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder builder)
        //{
        //    builder.UseNpgsql("Host=51.75.123.239;Port=3000;Database=EFCoreTest;Username=ubuntu;Password=J0ab#271106;");
        //}
    }
}
