using Domain.Entities.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistance.EntityConfiguration.Audit
{
    public class HttpTransactionLogBuilder : IEntityTypeConfiguration<HttpTransactionLog>
    {
        public void Configure(EntityTypeBuilder<HttpTransactionLog> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK_LogHttpTransactions");
            builder.Property(e => e.Method)
                .IsRequired()
                .HasColumnType("varchar(10)");

            builder.Property(e => e.Path)
                .IsRequired()
                .HasColumnType("varchar(250)");

            builder.Property(e => e.QueryString)
                .HasColumnType("varchar(500)");

            builder.Property(e => e.StatusCode)
                .IsRequired()
                .HasColumnType("int");

            builder.Property(e => e.Timestamp)
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("NOW()");

            builder.Property(e => e.Duration)
                .IsRequired()
                .HasColumnType("bigint");

            builder.ToTable("LogHttpTransactions", "audit");
        }
    }
}
