using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.HasKey(d => d.DocumentId);
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            builder.Property(d => d.FileType).HasMaxLength(50);
            builder.Property(d => d.FilePath).IsRequired().HasMaxLength(1000);
            builder.Property(d => d.Status).HasConversion<int>();
            builder.Property(d => d.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(d => d.UploadedByUserId);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(d => d.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.NotificationId);
            builder.Property(n => n.Event).HasConversion<int>();
            builder.Property(n => n.Channel).HasConversion<int>();
            builder.Property(n => n.Title).IsRequired().HasMaxLength(200);
            builder.Property(n => n.Message).HasMaxLength(4000);
            builder.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => n.IsRead);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class NotificationHistoryConfiguration : IEntityTypeConfiguration<NotificationHistory>
    {
        public void Configure(EntityTypeBuilder<NotificationHistory> builder)
        {
            builder.HasKey(h => h.HistoryId);
            builder.Property(h => h.Channel).HasConversion<int>();
            builder.Property(h => h.DeliveryStatus).HasConversion<int>();
            builder.Property(h => h.SentAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(h => h.FailureReason).HasMaxLength(1000);

            builder.HasIndex(h => h.NotificationId);

            builder.HasOne<Notification>()
                .WithMany()
                .HasForeignKey(h => h.NotificationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
