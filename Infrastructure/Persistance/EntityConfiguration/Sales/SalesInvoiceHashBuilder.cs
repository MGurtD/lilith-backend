using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class SalesInvoiceHashBuilder : IEntityTypeConfiguration<SalesInvoiceHash>
    {
        public const string TABLE_NAME = "SalesInvoiceHash";

        public void Configure(EntityTypeBuilder<SalesInvoiceHash> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.SalesInvoiceID)
                .IsRequired()
                .HasColumnType("uuid")
                .HasMaxLength(512);
            builder
                .Property(b => b.IDEmisorFactura)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.NumSerieFactura)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.FechaExpedicionFactura)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.TipoFactura)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);

            builder
                .Property(b => b.CuotaTotal)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.ImporteTotal)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.Huella)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.FechaHoraHusoGenRegistro)
                .IsRequired()
                .HasColumnType("timestamp")
                .HasMaxLength(512);
            builder
                .Property(b => b.IDEmisorFacturaAnulada)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.NumSerieFacturaAnulada)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(512);
            builder
                .Property(b => b.FechaExpedicionFacturaAnulada)
                .IsRequired()
                .HasColumnType("timestamp")
                .HasMaxLength(512);
            builder
                .Property(b => b.Response)
                .IsRequired()
                .HasColumnType("varchar");
            builder
                .Property(b => b.Response)
                .IsRequired()
                .HasColumnType("varchar");
            builder
                .Property(b => b.TimeStampResponse)
                .IsRequired()
                .HasColumnType("timestamp");

            builder
            .HasOne(sih => sih.SalesInvoice)
            .WithOne(si => si.SalesInvoiceHash) 
            .HasForeignKey<SalesInvoiceHash>(sih => sih.SalesInvoiceID) 
            .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

        }
    }
}
