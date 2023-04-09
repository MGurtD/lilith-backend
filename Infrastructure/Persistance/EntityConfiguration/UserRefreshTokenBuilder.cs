using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Xml;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public class UserRefreshTokenBuilder : IEntityTypeConfiguration<UserRefreshToken>
    {
        public void Configure(EntityTypeBuilder<UserRefreshToken> builder)
        {
            builder.ConfigureBase();

            builder.Property(b => b.JwtId)
                   .ValueGeneratedNever()
                   .HasColumnType("uuid");
            builder
                .Property(b => b.Token)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(600);
            builder
                .Property(b => b.IsActive)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(true);
            builder
                .Property(b => b.IsRevoked)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            builder
                .Property(e => e.ExpiryDate)
                .HasColumnType<DateTime>("timestamp")
                .IsRequired();

            builder
                .HasKey(b => b.Id)
                .HasName("PK_UserRefreshToken");

            builder.HasIndex(builder => builder.JwtId, "UK_UserRefreshToken_JwtId");

            builder.ToTable("UserRefreshTokens");
        }

    }
}
