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

            builder
                .Property(b => b.JwtId)
                .ValueGeneratedNever()
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.Token)
                .ValueGeneratedNever()
                .IsRequired()
                .HasColumnType("uuid");
            builder
                .Property(b => b.Used)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            builder
                .Property(b => b.Revoked)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
            builder
                .Property(e => e.ExpiryDate)
                .IsRequired();

            builder
                .HasKey(b => b.Id)
                .HasName("PK_UserRefreshToken");

            builder.HasIndex(builder => builder.JwtId, "UK_UserRefreshToken_JwtId");

            builder.ToTable("UserRefreshTokens");
        }

    }
}
