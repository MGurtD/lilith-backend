using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities.Warehouse;

namespace Infrastructure.Persistance.EntityConfiguration.Warehouse
{
    public class ReferenceTypeBuilder : IEntityTypeConfiguration<ReferenceType>
    {
        public void Configure(EntityTypeBuilder<ReferenceType> builder)
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
                .Property(b => b.PrimaryColor)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(25);
            builder
                .Property(b => b.SecondaryColor)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(25);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_ReferenceType");
            builder.HasIndex(builder => builder.Name, "UK_ReferenceType_Name");

            builder.ToTable("ReferenceTypes");
        }
    }
}