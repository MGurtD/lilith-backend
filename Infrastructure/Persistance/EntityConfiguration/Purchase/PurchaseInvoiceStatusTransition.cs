using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Purchase;

namespace Infrastructure.Persistance.EntityConfiguration.Purchase
{
    public class PurchaseInvoiceStatusTransitionBuilder : IEntityTypeConfiguration<PurchaseInvoiceStatusTransition>
    {
        public const string TABLE_NAME = "PurchaseInvoiceStatusTransitions";

        public void Configure(EntityTypeBuilder<PurchaseInvoiceStatusTransition> builder)
        {
            builder.ConfigureBase();

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder.ToTable(TABLE_NAME);
        }

    }
}
