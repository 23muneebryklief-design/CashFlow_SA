using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
    {
        public void Configure(EntityTypeBuilder<UserSession> builder)
        {
            builder.HasKey(s => s.SessionId);
            builder.Property(s => s.DeviceInformation).HasMaxLength(500);
            builder.Property(s => s.IPAddress).HasMaxLength(45);
            builder.Property(s => s.RefreshToken).HasMaxLength(512);
            builder.Property(s => s.LoginTimestamp).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.RefreshToken).IsUnique();

            builder.HasOne(s => s.User)
                .WithMany(u => u.UserSessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class KYCApplicationConfiguration : IEntityTypeConfiguration<KYCApplication>
    {
        public void Configure(EntityTypeBuilder<KYCApplication> builder)
        {
            builder.HasKey(k => k.ApplicationId);
            builder.Property(k => k.ApplicationDate).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(k => k.Status).HasConversion<string>().HasMaxLength(15);

            builder.HasIndex(k => k.SMEId);
            builder.HasIndex(k => k.Status);

            builder.HasOne(k => k.SME)
                .WithMany(s => s.KYCApplications)
                .HasForeignKey(k => k.SMEId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class KYCReviewConfiguration : IEntityTypeConfiguration<KYCReview>
    {
        public void Configure(EntityTypeBuilder<KYCReview> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Outcome).HasConversion<string>().HasMaxLength(15);
            builder.Property(r => r.Notes).HasMaxLength(4000);
            builder.Property(r => r.ReviewDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(r => r.KYCApplicationId);
            builder.HasIndex(r => r.ReviewerId);

            builder.HasOne<KYCApplication>()
                .WithMany()
                .HasForeignKey(r => r.KYCApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class KYCDocumentConfiguration : IEntityTypeConfiguration<KYCDocuments>
    {
        public void Configure(EntityTypeBuilder<KYCDocuments> builder)
        {
            builder.HasKey(d => d.DocumentId);
            builder.Property(d => d.DocumentType).HasConversion<string>().HasMaxLength(25);
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            builder.Property(d => d.FilePath).IsRequired().HasMaxLength(1000);
            builder.Property(d => d.UploadedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(d => d.FileSize).HasDefaultValue(0);
            builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(15);

            builder.HasIndex(d => d.UserId);
            builder.HasIndex(d => d.Status);
        }
    }
}
