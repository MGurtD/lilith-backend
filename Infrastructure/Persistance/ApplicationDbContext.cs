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
            new EnterpriseBuilder().Configure(builder.Entity<Enterprise>());
            new SiteBuilder().Configure(builder.Entity<Site>());


        }

    }
}
