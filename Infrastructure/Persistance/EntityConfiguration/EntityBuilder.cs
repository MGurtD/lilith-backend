using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration
{
    public static class EntityBaseConfiguration
    {
        public static void ConfigureBase<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : Entity
        {
            builder
                .Property(b => b.Id)
                .ValueGeneratedNever()
                .HasColumnType("uuid");
            builder
                .Property(e => e.CreatedOn)
                .HasColumnType("timestamp without time zone")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("NOW()");
            builder
                .Property(e => e.UpdatedOn)
                .HasColumnType("timestamp without time zone")
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("NOW()");
            builder
                .Property(b => b.Disabled)
                .IsRequired()
                .HasColumnType("bool")
                .HasDefaultValue(false);
        }
    }
}
