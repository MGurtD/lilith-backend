using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistance.EntityConfiguration.Production
{
    public class MachineStatusBuilder : IEntityTypeConfiguration<MachineStatus>
    {
        public void Configure(EntityTypeBuilder<MachineStatus> builder)
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
                .Property(b => b.Color)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(20);
            builder
                .HasKey(b => b.Id)
                .HasName("PK_MachineStatus");
            builder.HasIndex(builder => builder.Name, "UK_MachineStatus_Name");

            builder.ToTable("MachineStatuses");
        }
    }
}
