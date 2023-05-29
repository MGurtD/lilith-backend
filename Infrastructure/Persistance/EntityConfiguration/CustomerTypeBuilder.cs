using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class CustomerTypeBuilder : IEntityTypeConfiguration<CustomerType>
    {
        public void Configure(EntityTypeBuilder<CustomerType> builder) 
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .Property(b => b.Disabled)
                .HasColumnType("boolean");

            builder
                .HasKey(b => b.Id)
                .HasName("PK_CustomerType");

            builder
                .HasIndex(builder => builder.Name, "UK_CustomerType_Name")
                .IsUnique();

            builder.ToTable("CustomerTypes", "Config");

        }
    }
}
