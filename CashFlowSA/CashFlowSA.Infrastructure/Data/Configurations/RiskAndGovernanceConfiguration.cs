using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class UnderwritingReviewConfiguration : IEntityTypeConfiguration<UnderwritingReview>
    {
        public void Configure(EntityTypeBuilder<UnderwritingReview> builder)
        {
            builder.HasKey(r => r.ReviewId);
            builder.Property(r => r.Decision).HasConversion<string>().HasMaxLength(35);
            builder.Property(r => r.Notes).HasMaxLength(2000);
            builder.Property(r => r.RiskJustification).HasMaxLength(4000);
            builder.Property(r => r.ReviewDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(r => r.FundingRequestId);
            builder.HasIndex(r => r.ReviewerId);

            builder.HasOne<FundingRequest>()
                .WithMany()
                .HasForeignKey(r => r.FundingRequestId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RiskAssessmentConfiguration : IEntityTypeConfiguration<RiskAssessment>
    {
        public void Configure(EntityTypeBuilder<RiskAssessment> builder)
        {
            builder.HasKey(r => r.RiskAssessmentId);
            builder.Property(r => r.RiskScore).HasPrecision(5, 2);
            builder.Property(r => r.RiskGrade).HasConversion<string>().HasMaxLength(5);
            builder.Property(r => r.ScoringFactors).HasMaxLength(4000);
            builder.Property(r => r.AssessedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(r => r.InvoiceId).IsUnique();

            builder.HasOne<Invoice>()
                .WithMany()
                .HasForeignKey(r => r.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class AIExplanationConfiguration : IEntityTypeConfiguration<AIExplanation>
    {
        public void Configure(EntityTypeBuilder<AIExplanation> builder)
        {
            builder.HasKey(e => e.AIExplanationId);
            builder.Property(e => e.ExplanationText).HasMaxLength(4000);
            builder.Property(e => e.InvestmentSummary).HasMaxLength(4000);
            builder.Property(e => e.ModelUsed).HasMaxLength(200);
            builder.Property(e => e.GeneratedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(e => e.RiskAssessmentId).IsUnique();

            builder.HasOne<RiskAssessment>()
                .WithMany()
                .HasForeignKey(e => e.RiskAssessmentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class RiskScoreHistoryConfiguration : IEntityTypeConfiguration<RiskScoreHistory>
    {
        public void Configure(EntityTypeBuilder<RiskScoreHistory> builder)
        {
            builder.HasKey(h => h.RiskScoreHistoryId);
            builder.Property(h => h.PreviousScore).HasPrecision(5, 2);
            builder.Property(h => h.PreviousGrade).HasConversion<string>().HasMaxLength(5);
            builder.Property(h => h.NewScore).HasPrecision(5, 2);
            builder.Property(h => h.NewGrade).HasConversion<string>().HasMaxLength(5);
            builder.Property(h => h.Reason).HasMaxLength(4000);
            builder.Property(h => h.ChangedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(h => h.InvoiceId);
            builder.HasIndex(h => h.ChangedByUserId);

            builder.HasOne<Invoice>()
                .WithMany()
                .HasForeignKey(h => h.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(h => h.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
