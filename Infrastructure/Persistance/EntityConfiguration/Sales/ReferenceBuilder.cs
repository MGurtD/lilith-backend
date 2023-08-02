﻿using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class ReferenceBuilder : IEntityTypeConfiguration<Reference>
    {
        public void Configure(EntityTypeBuilder<Reference> builder)
        {
            builder.ConfigureBase();
            builder
                .Property(b => b.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);
            builder
                .Property(b => b.Cost)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Price)
                .IsRequired()
                .HasColumnType("decimal")
                .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                              ApplicationDbContextConstants.DECIMAL_SCALE);
            builder
                .Property(b => b.Version)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);
                
            builder
                .HasKey(b => b.Id)
                .HasName("PK_Reference");
            
            builder.HasIndex(builder => new { builder.Code, builder.Version }, "UK_Reference_Code_Version");

            builder.ToTable("References");
        }
    }
}
