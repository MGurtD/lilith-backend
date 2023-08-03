using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class SalesOrderDetailBuilder : IEntityTypeConfiguration<SalesOrderDetail>
    {
        public void Configure(EntityTypeBuilder<SalesOrderDetail> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Quantity)
                .IsRequired()
                .HasColumnType("integer");
            builder
                .Property(b => b.UnitCost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.TotalCost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Amount)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.EstimatedDeliveryDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.IsServed)
                .IsRequired()
                .HasColumnType("boolean");
            builder
                .Property(b => b.IsInvoiced)
                .IsRequired()
                .HasColumnType("boolean");
        }
    }
}
