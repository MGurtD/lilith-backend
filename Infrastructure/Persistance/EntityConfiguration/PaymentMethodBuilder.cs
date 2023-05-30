using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class PaymentMethodBuilder : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
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
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            builder
                 .Property(b => b.DaysToAdd)
                 .IsRequired()
                 .HasColumnType("int")
                 .HasDefaultValue(0);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_PaymentMethod");

            builder.HasIndex(builder => builder.Name, "UK_PaymentMethod_Name");

            builder.ToTable("PaymentMethods", "Config");
        }

    }
}
