using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class SalesInvoiceVerifactuRequestBuilder : IEntityTypeConfiguration<SalesInvoiceVerifactuRequest>
    {
        public void Configure(EntityTypeBuilder<SalesInvoiceVerifactuRequest> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.SalesInvoiceId)
                .IsRequired()
                .HasColumnType("uuid")
                .HasMaxLength(512);
            builder
                .Property(b => b.Request)
                .IsRequired()
                .HasColumnType("text");
            builder
                .Property(b => b.Response)
                .IsRequired()
                .HasColumnType("varchar");
            builder
                .Property(b => b.Response)
                .IsRequired()
                .HasColumnType("varchar");
            builder
                .Property(b => b.TimestampResponse)
                .IsRequired()
                .HasColumnType("timestamp");

            builder
            .HasOne(sih => sih.SalesInvoice)
            .WithOne(si => si.VerifactuRequest)
            .HasForeignKey<SalesInvoiceVerifactuRequest>(sih => sih.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{nameof(SalesInvoiceVerifactuRequest)}");

        }
    }
}
