using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(w => w.WalletId);
            builder.Property(w => w.Balance).HasPrecision(18, 2);
            builder.Property(w => w.Currency).IsRequired().HasMaxLength(10);
            builder.Property(w => w.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(w => w.UserId).IsUnique();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> builder)
        {
            builder.HasKey(t => t.TransactionId);
            builder.Property(t => t.Type).HasConversion<int>();
            builder.Property(t => t.Amount).HasPrecision(18, 2);
            builder.Property(t => t.ReferenceType).HasMaxLength(100);
            builder.Property(t => t.Description).HasMaxLength(500);
            builder.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(t => t.WalletId);

            builder.HasOne<Wallet>()
                .WithMany()
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class SettlementConfiguration : IEntityTypeConfiguration<Settlement>
    {
        public void Configure(EntityTypeBuilder<Settlement> builder)
        {
            builder.HasKey(s => s.SettlementId);
            builder.Property(s => s.SettledAmount).HasPrecision(18, 2);
            builder.Property(s => s.Status).HasConversion<int>();
            builder.Property(s => s.PaymentProvider).HasMaxLength(100);
            builder.Property(s => s.ReferenceNumber).HasMaxLength(100);
            builder.Property(s => s.SettlementDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(s => s.CampaignId).IsUnique();

            builder.HasOne<FundingCampaign>()
                .WithMany()
                .HasForeignKey(s => s.CampaignId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ReturnDistributionConfiguration : IEntityTypeConfiguration<ReturnDistribution>
    {
        public void Configure(EntityTypeBuilder<ReturnDistribution> builder)
        {
            builder.HasKey(r => r.ReturnDistributionId);
            builder.Property(r => r.PrincipalAmount).HasPrecision(18, 2);
            builder.Property(r => r.ReturnAmount).HasPrecision(18, 2);
            builder.Property(r => r.DistributedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(r => r.SettlementId);
            builder.HasIndex(r => r.InvestmentId);
            builder.HasIndex(r => r.InvestorId);

            builder.HasOne<Settlement>()
                .WithMany()
                .HasForeignKey(r => r.SettlementId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Investment>()
                .WithMany()
                .HasForeignKey(r => r.InvestmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Investor>()
                .WithMany()
                .HasForeignKey(r => r.InvestorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
