using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Warehouse;

namespace Infrastructure.Persistance.EntityConfiguration.Warehouse
{
    public class RawMaterialTypeBuilder : IEntityTypeConfiguration<RawMaterialType>
    {
        public void Configure(EntityTypeBuilder<RawMaterialType> builder)
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
                .HasKey(b => b.Id)
                .HasName("PK_RawMaterialType");
            builder.HasIndex(builder => builder.Name, "UK_RawMaterialType_Name");

            builder.ToTable("RawMaterialTypes");
        }
    }
}