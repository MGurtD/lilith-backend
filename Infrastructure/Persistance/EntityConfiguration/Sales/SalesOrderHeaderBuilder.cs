﻿using Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Sales
{
    public class SalesOrderHeaderBuilder : IEntityTypeConfiguration<SalesOrderHeader>
    {
        public void Configure(EntityTypeBuilder<SalesOrderHeader> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.SalesOrderDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.SalesOrderNumber)
                .IsRequired()
                .HasColumnType("integer");
            builder
                .Property(b => b.CustomerCode)                
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);

            builder
                .Property(b => b.CustomerSalesOrderNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.CustomerComercialName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);

            builder
                .Property(b => b.CustomerTaxName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);

            builder
                .Property(b => b.CustomerVatNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .Property(b => b.CustomerAccountNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(35);
            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Address)
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
                .HasMaxLength(250);
            builder
                .Property(b => b.Region)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Country)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.VatNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(12);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_SalesOrderHeader");
            builder
                .HasIndex(builder => builder.ExerciseId, "IDX_SalesOrderHeader_Exercise");
            builder
                .HasIndex(builder => builder.CustomerId, "IDX_SalesOrderHeader_Customer");
        }
    }
}
