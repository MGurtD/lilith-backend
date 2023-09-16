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
    public class OperatorCostBuilder : IEntityTypeConfiguration<OperatorCost>
    {
        public void Configure(EntityTypeBuilder<OperatorCost> builder) 
        {
            builder.ConfigureBase();
            builder
               .Property(b => b.Cost)
               .IsRequired()
               .HasColumnType("decimal")
               .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                             ApplicationDbContextConstants.DECIMAL_SCALE);

            builder
               .HasKey(b => b.Id)
               .HasName("PK_OperatorCost");
            builder
                .HasIndex(builder => new { builder.OperatorTypeId, builder.MachineStatusId }, $"UK_OperatorCosts_Operator_MachineStatus")
                .IsUnique();

            builder.ToTable("OperatorCosts");
        }
    }
}
