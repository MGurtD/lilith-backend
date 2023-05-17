using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class SupplierBuilder : IEntityTypeConfiguration<Supplier>
    {
        public void Configure(EntityTypeBuilder<Supplier> builder)
        {
            builder.ConfigureBase();

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
                .Property(b => b.CIF)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .Property(b => b.Country)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Region)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.City)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);            
            builder
                .Property(b => b.PostalCode)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);
            builder
                .Property(b => b.Address)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);
            builder
                .Property(b => b.Phone)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_Supplier");

            builder
                .HasIndex(builder => builder.ComercialName, "UK_Supplier_Name")
                .IsUnique();

            builder.ToTable("Suppliers", "Config");
        }

    }
}
