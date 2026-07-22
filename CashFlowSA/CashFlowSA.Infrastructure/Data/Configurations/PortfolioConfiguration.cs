using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class InvestorPortfolioConfiguration : IEntityTypeConfiguration<InvestorPortfolio>
    {
        public void Configure(EntityTypeBuilder<InvestorPortfolio> builder)
        {
            builder.HasKey(p => p.PortfolioId);
            builder.Property(p => p.TotalCommitted).HasPrecision(18, 2);
            builder.Property(p => p.TotalFunded).HasPrecision(18, 2);
            builder.Property(p => p.TotalReturned).HasPrecision(18, 2);
            builder.Property(p => p.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(p => p.InvestorId).IsUnique();
        }
    }
}
