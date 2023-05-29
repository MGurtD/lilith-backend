using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class CustomerBuilder : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder) 
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);

            builder
                .Property(b => b.ComercialName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);

            builder
                .Property(b => b.TaxName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);

            builder
                .Property(b => b.VatNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);

            builder
                .Property(b => b.Web)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(150);

            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_Customer");

            builder
                .HasIndex(builder => builder.ComercialName, "UK_Customer_Name")
                .IsUnique();
            
            builder
                .HasIndex(builder => builder.VatNumber, "UK_Customer_VatNumber")
                .IsUnique();

            builder.ToTable("Customers", "Config");
        }
    }
}
