using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class PurchaseInvoiceStatusBuilder : IEntityTypeConfiguration<PurchaseInvoiceStatus>
    {
        public const string TABLE_NAME = "PurchaseInvoiceStatuses";

        public void Configure(EntityTypeBuilder<PurchaseInvoiceStatus> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(builder => builder.Name, $"UK_{TABLE_NAME}_Name")
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }

    }
}
