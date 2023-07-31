using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class StatusEntityBuilder : IEntityTypeConfiguration<Lifecycle>
    {
        public const string TABLE_NAME = "Lifecycles";

        public void Configure(EntityTypeBuilder<Lifecycle> builder)
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
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(builder => builder.Name, $"UK_{TABLE_NAME}_Name")
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }

    }
}
