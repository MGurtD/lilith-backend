using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Production;
public class WorkMasterPhaseBuilder : IEntityTypeConfiguration<WorkMasterPhase>
{
    public const string TABLE_NAME = "WorkMasterPhase";
    public void Configure(EntityTypeBuilder<WorkMasterPhase> builder)
    {
        builder.ConfigureBase();
        builder
            .Property(b => b.PhaseCode)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(10);
        builder
            .Property(b => b.PhaseDescription)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(250);
        builder
            .HasKey(b => b.Id)
            .HasName($"PK_{TABLE_NAME}");

        builder.ToTable(TABLE_NAME);
    }
}
