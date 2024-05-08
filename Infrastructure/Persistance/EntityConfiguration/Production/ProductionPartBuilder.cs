﻿using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class ProductionPartBuilder : IEntityTypeConfiguration<ProductionPart>
    {
        public void Configure(EntityTypeBuilder<ProductionPart> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Date)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.Quantity)
                .IsRequired()
                .HasColumnType("int");
            builder
                .Property(b => b.Time)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.OperatorHourCost)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.MachineHourCost)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_ProductionParts");
            builder
                .HasIndex(b => new { b.WorkcenterId, b.OperatorId, b.WorkOrderPhaseDetailId })
                .HasDatabaseName("idx_workcenter_phasedetail_operator");
            builder
                .HasIndex(b => b.Date)
                .HasDatabaseName("idx_productionpart_date");

            builder.ToTable("ProductionParts");
                
        }
    }
}
