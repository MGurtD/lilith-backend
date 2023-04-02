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
            builder.Property(b => b.Id)
                   .ValueGeneratedNever()
                   .HasColumnType("uuid");
            builder.Property(e => e.CreatedOn)
                  .ValueGeneratedOnAdd()
                  .HasDefaultValueSql("NOW()");
            builder.Property(e => e.CreatedOn)
                  .ValueGeneratedOnUpdate()
                  .HasDefaultValueSql("NOW()");
        }
    }
}
