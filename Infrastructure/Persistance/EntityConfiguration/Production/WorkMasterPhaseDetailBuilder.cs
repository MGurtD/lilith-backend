using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkMasterPhaseDetailBuilder : IEntityTypeConfiguration<WorkMasterPhaseDetail>
{
    public const string TABLE_NAME = "WorkMasterPhaseDetail";
    public void Configure(EntityTypeBuilder<WorkMasterPhaseDetail> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.EstimatedTime)
            .IsRequired()
            .HasColumnType("decimal")
            .HasPrecision(ApplicationDbContextConstants.DECIMAL_PRECISION,
                            ApplicationDbContextConstants.DECIMAL_SCALE);
        builder
            .Property(b => b.IsCycleTime)
            .HasColumnType("boolean");
        builder
            .Property(b => b.IsExternalWork)
            .HasColumnType("boolean");

        builder
            .Property(b => b.ExternalWorkCost)
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
