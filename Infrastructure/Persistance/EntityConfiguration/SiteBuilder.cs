using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class SiteBuilder : IEntityTypeConfiguration<Site>
    {
        public void Configure(EntityTypeBuilder<Site> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(50);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);
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
                .Property(b => b.PhoneNumber)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(25);
            builder
                .Property(b => b.Email)
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
                .HasName("PK_Site");

            builder.HasIndex(builder => builder.Name, "UK_Site_Name");

            builder.ToTable("Sites");
        }

    }
}
