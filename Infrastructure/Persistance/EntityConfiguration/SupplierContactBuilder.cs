using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class SupplierContactBuilder : IEntityTypeConfiguration<SupplierContact>
    {
        public void Configure(EntityTypeBuilder<SupplierContact> builder)
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
                .Property(b => b.Phone)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .Property(b => b.PhoneExtension)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(5);
            builder
                .Property(b => b.Observations)
                .IsRequired()
                .HasColumnType("text");
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);

            builder
                .HasKey(b => b.Id)
                .HasName("PK_SupplierContact");

            builder.ToTable("SupplierContacts");
        }

    }
}
