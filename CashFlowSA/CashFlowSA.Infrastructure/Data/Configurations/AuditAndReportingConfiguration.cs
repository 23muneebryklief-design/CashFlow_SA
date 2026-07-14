using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.AuditLogId);
            builder.Property(a => a.Action).HasConversion<int>();
            builder.Property(a => a.EntityType).IsRequired().HasMaxLength(200);
            builder.Property(a => a.IPAddress).HasMaxLength(45);
            builder.Property(a => a.Timestamp).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.Timestamp);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class GeneratedReportConfiguration : IEntityTypeConfiguration<GeneratedReport>
    {
        public void Configure(EntityTypeBuilder<GeneratedReport> builder)
        {
            builder.HasKey(r => r.ReportId);
            builder.Property(r => r.ReportName).IsRequired().HasMaxLength(200);
            builder.Property(r => r.ReportType).HasConversion<int>();
            builder.Property(r => r.FilePath).HasMaxLength(1000);
            builder.Property(r => r.Description).HasMaxLength(4000);
            builder.Property(r => r.GeneratedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(r => r.GeneratedByUserId);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.GeneratedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
