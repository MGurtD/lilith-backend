﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class ExerciseBuilder : IEntityTypeConfiguration<Exercise>
    {
        public const string TABLE_NAME = "Exercises";

        public void Configure(EntityTypeBuilder<Exercise> builder)
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
                .Property(b => b.StartDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");
            builder
                .Property(b => b.EndDate)
                .IsRequired()
                .HasColumnType("timestamp without time zone");

            builder
                .Property(b => b.PurchaseInvoiceCounter)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("integer");
            builder
                .Property(b => b.SalesInvoiceCounter)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("integer");
            builder
                .Property(b => b.SalesOrderCounter)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("integer");
            builder
                .Property(b => b.DeliveryNoteCounter)
                .IsRequired()
                .HasDefaultValue(0)
                .HasColumnType("integer");            

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
