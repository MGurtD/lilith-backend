using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class CustomerAddressBuilder : IEntityTypeConfiguration<CustomerAddress>
    {
        public void Configure(EntityTypeBuilder<CustomerAddress> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .Property(b => b.MainAddress)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .Property(b => b.Address)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .Property(b => b.AddressExtraInfo)
                .IsRequired()
                .HasColumnType("text");
            builder
                .Property(b => b.Country)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
               .Property(b => b.Region)
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
                .HasMaxLength(10);
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_CustomerAddress");

            builder
                .HasIndex(builder => builder.Name, "UK_CustomerAddress_Name");

            builder.ToTable("CustomerAddress", "Config");
        }
    }
}
