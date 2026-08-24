using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.HasKey(i => i.InvoiceId);

            builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(100);
            builder.Property(i => i.DebtorName).IsRequired().HasMaxLength(200);
            builder.Property(i => i.DebtorContactDetails).HasMaxLength(500);
            builder.Property(i => i.Amount).HasPrecision(18, 2);
            builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
            builder.Property(i => i.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(i => i.ReviewNotes).HasMaxLength(4000);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Invoice_Amount_Positive",
                "[Status] = 'Draft' OR [Amount] > 0"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Invoice_DueDate_NotBeforeIssueDate",
                "[DueDate] >= [IssueDate]"));

            builder.HasIndex(i => i.InvoiceNumber).IsUnique();
            builder.HasIndex(i => i.SMEId);
            builder.HasIndex(i => i.Status);
            builder.HasIndex(i => i.ReviewedByUserId);

            builder.HasOne<SME>()
                .WithMany()
                .HasForeignKey(i => i.SMEId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InvoiceDocumentConfiguration : IEntityTypeConfiguration<InvoiceDocument>
    {
        public void Configure(EntityTypeBuilder<InvoiceDocument> builder)
        {
            builder.HasKey(d => d.InvoiceDocumentId);
            builder.Property(d => d.FileName).IsRequired().HasMaxLength(255);
            builder.Property(d => d.FilePath).IsRequired().HasMaxLength(1000);
            builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(15);
            builder.Property(d => d.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(d => d.InvoiceId);

            builder.HasOne<Invoice>()
                .WithMany()
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class OCRResultConfiguration : IEntityTypeConfiguration<OCRResult>
    {
        public void Configure(EntityTypeBuilder<OCRResult> builder)
        {
            builder.HasKey(o => o.OCRResultId);
            builder.Property(o => o.ExtractedInvoiceNumber).HasMaxLength(100);
            builder.Property(o => o.ExtractedDebtorName).HasMaxLength(200);
            builder.Property(o => o.ExtractedAmount).HasPrecision(18, 2);
            builder.Property(o => o.ConfidenceScore).HasPrecision(5, 2);
            builder.Property(o => o.ProcessedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_OCRResult_ConfidenceScore_Valid",
                "[ConfidenceScore] >= 0 AND [ConfidenceScore] <= 100"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_OCRResult_ExtractedAmount_NonNegative",
                "[ExtractedAmount] IS NULL OR [ExtractedAmount] >= 0"));

            builder.HasIndex(o => o.InvoiceId).IsUnique();

            builder.HasOne<Invoice>()
                .WithMany()
                .HasForeignKey(o => o.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
