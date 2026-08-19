using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class FundingRequestConfiguration : IEntityTypeConfiguration<FundingRequest>
    {
        public void Configure(EntityTypeBuilder<FundingRequest> builder)
        {
            builder.HasKey(r => r.FundingRequestId);
            builder.Property(r => r.RequestedAmount).HasPrecision(18, 2);
            builder.Property(r => r.FundingModel).HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
            builder.Property(r => r.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");
            builder.Property(r => r.ReviewNotes).HasMaxLength(4000);

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_FundingRequest_RequestedAmount_Positive",
                "[RequestedAmount] > 0"));

            builder.HasIndex(r => r.InvoiceId);
            builder.HasIndex(r => r.SMEId);
            builder.HasIndex(r => r.Status);

            builder.HasOne<Invoice>()
                .WithMany()
                .HasForeignKey(r => r.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<SME>()
                .WithMany()
                .HasForeignKey(r => r.SMEId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class FundingCampaignConfiguration : IEntityTypeConfiguration<FundingCampaign>
    {
        public void Configure(EntityTypeBuilder<FundingCampaign> builder)
        {
            builder.HasKey(c => c.CampaignId);
            builder.Property(c => c.FundingModel).HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.TargetAmount).HasPrecision(18, 2);
            builder.Property(c => c.ExpectedReturnRate).HasPrecision(5, 2);
            builder.Property(c => c.FundedAmount).HasPrecision(18, 2);
            builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(15);
            builder.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_FundingCampaign_Amounts_Valid",
                "[TargetAmount] > 0 AND [FundedAmount] >= 0 AND [FundedAmount] <= [TargetAmount]"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_FundingCampaign_ReturnRate_Valid",
                "[ExpectedReturnRate] IS NULL OR ([ExpectedReturnRate] >= 0 AND [ExpectedReturnRate] <= 100)"));

            builder.HasIndex(c => c.FundingRequestId);
            builder.HasIndex(c => c.InvoiceId);
            builder.HasIndex(c => c.SMEId);
            builder.HasIndex(c => c.Status);

            builder.HasOne<FundingRequest>()
                .WithMany()
                .HasForeignKey(c => c.FundingRequestId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Invoice>()
                .WithMany()
                .HasForeignKey(c => c.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<SME>()
                .WithMany()
                .HasForeignKey(c => c.SMEId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class MarketplaceListingConfiguration : IEntityTypeConfiguration<MarketplaceListing>
    {
        public void Configure(EntityTypeBuilder<MarketplaceListing> builder)
        {
            builder.HasKey(l => l.ListingId);
            builder.Property(l => l.RiskGrade).HasConversion<string>().HasMaxLength(5);
            builder.Property(l => l.RiskScore).HasPrecision(5, 2);
            builder.Property(l => l.Industry).HasConversion<string>().HasMaxLength(30);
            builder.Property(l => l.PublishedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(l => l.CampaignId).IsUnique();
            builder.HasIndex(l => l.IsActive);

            builder.HasOne<FundingCampaign>()
                .WithMany()
                .HasForeignKey(l => l.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class AuctionBidConfiguration : IEntityTypeConfiguration<AuctionBid>
    {
        public void Configure(EntityTypeBuilder<AuctionBid> builder)
        {
            builder.HasKey(b => b.BidId);
            builder.Property(b => b.BidAmount).HasPrecision(18, 2);
            builder.Property(b => b.ProposedReturnRate).HasPrecision(5, 2);
            builder.Property(b => b.Status).HasConversion<string>().HasMaxLength(15);
            builder.Property(b => b.SubmittedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_AuctionBid_Amount_Positive",
                "[BidAmount] > 0"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_AuctionBid_ReturnRate_Valid",
                "[ProposedReturnRate] >= 0 AND [ProposedReturnRate] <= 100"));

            builder.HasIndex(b => b.CampaignId);
            builder.HasIndex(b => b.InvestorId);

            builder.HasOne<FundingCampaign>()
                .WithMany()
                .HasForeignKey(b => b.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Investor>()
                .WithMany()
                .HasForeignKey(b => b.InvestorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
    {
        public void Configure(EntityTypeBuilder<Investment> builder)
        {
            builder.HasKey(i => i.InvestmentId);
            builder.Property(i => i.Amount).HasPrecision(18, 2);
            builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(15);
            builder.Property(i => i.ReturnAmount).HasPrecision(18, 2);
            builder.Property(i => i.InvestedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Investment_Amount_Positive",
                "[Amount] > 0"));

            builder.ToTable(t => t.HasCheckConstraint(
                "CK_Investment_ReturnAmount_NonNegative",
                "[ReturnAmount] IS NULL OR [ReturnAmount] >= 0"));

            builder.HasIndex(i => i.CampaignId);
            builder.HasIndex(i => i.InvestorId);

            builder.HasOne<FundingCampaign>()
                .WithMany()
                .HasForeignKey(i => i.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Investor>()
                .WithMany()
                .HasForeignKey(i => i.InvestorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}