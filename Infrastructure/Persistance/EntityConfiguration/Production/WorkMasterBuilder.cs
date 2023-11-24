using Domain.Entities.Production;
using Infrastructure.Persistance.EntityConfiguration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkMasterBuilder : IEntityTypeConfiguration<WorkMaster>
{
    public const string TABLE_NAME = "WorkMaster";

    public void Configure(EntityTypeBuilder<WorkMaster> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.BaseQuantity)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                           ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
                .HasKey(b => b.Id)
                .HasName($"PK_{TABLE_NAME}");

        builder.ToTable(TABLE_NAME);
    }
}
