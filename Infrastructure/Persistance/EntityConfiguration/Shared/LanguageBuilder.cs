using Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Shared
{
    public class LanguageBuilder : IEntityTypeConfiguration<Language>
    {
        public const string TABLE_NAME = "Languages";

        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.ConfigureBase();

            builder
                .Property(l => l.Code)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(10);

            builder
                .Property(l => l.Name)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(100);

            builder
                .Property(l => l.Icon)
                .HasColumnType("varchar")
                .HasMaxLength(255);

            builder
                .Property(l => l.IsDefault)
                .IsRequired()
                .HasColumnType("boolean")
                .HasDefaultValue(false);

            builder
                .Property(l => l.SortOrder)
                .IsRequired()
                .HasColumnType("integer")
                .HasDefaultValue(0);

            builder
                .HasKey(l => l.Id)
                .HasName($"PK_{TABLE_NAME}");

            builder
                .HasIndex(l => l.Code, $"UK_{TABLE_NAME}_Code")
                .IsUnique();

            builder.ToTable(TABLE_NAME);
        }
    }
}