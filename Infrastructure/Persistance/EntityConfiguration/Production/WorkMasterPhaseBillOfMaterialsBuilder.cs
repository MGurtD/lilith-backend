using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkMasterPhaseBillOfMaterialsBuilder : IEntityTypeConfiguration<WorkMasterPhaseBillOfMaterials>
{
    public const string TABLE_NAME = "WorkMasterPhaseBillOfMaterials";
    public void Configure(EntityTypeBuilder<WorkMasterPhaseBillOfMaterials> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.Quantity)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                            ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.Waste)
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
