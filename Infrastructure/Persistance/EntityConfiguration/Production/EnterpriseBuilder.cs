using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using Domain.Entities.Production;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class EnterpriseBuilder : IEntityTypeConfiguration<Enterprise>
    {
        public void Configure(EntityTypeBuilder<Enterprise> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);
            builder
                .Property(b => b.Description)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(250);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_Enterprise");

            builder.HasIndex(builder => builder.Name, "UK_Enterprise_Name");

            builder.ToTable("Enterprises");
        }

    }
}
