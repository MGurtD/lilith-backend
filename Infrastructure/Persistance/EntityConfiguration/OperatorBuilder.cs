using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class OperatorBuilder : IEntityTypeConfiguration<Operator>
    {

        public void Configure(EntityTypeBuilder<Operator> builder)
        {
            builder
                .Property(b => b.Id)
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");
            builder
                .Property(b => b.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(500);
            builder
                .Property(b => b.Cost)
                .HasDefaultValue(0)
                .HasPrecision(8, 2)
                .IsRequired();
            builder
                .Property(b => b.Type)
                .IsRequired()
                .HasDefaultValue("")
                .HasColumnType("varchar")
                .HasMaxLength(500);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_Operators");

            builder.ToTable("Operators", "Config");
        }

    }
}
