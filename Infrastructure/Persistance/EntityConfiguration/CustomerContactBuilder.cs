using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class CustomerContactBuilder : IEntityTypeConfiguration<CustomerContact>
    {
        public void Configure(EntityTypeBuilder<CustomerContact> builder) 
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.FirstName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.LastName)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Charge)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.Email)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
            builder
                .Property(b => b.PhoneNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            builder
                .Property(b => b.MainContact)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_CustomerContact");

            builder.ToTable("CustomerContacts", "Config");

        }
    }
}
