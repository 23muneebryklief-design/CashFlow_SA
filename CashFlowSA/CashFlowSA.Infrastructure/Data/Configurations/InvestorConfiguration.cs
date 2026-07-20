using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class InvestorConfiguration : IEntityTypeConfiguration<Investor>
    {
        public void Configure(EntityTypeBuilder<Investor> builder)
        {
            builder.HasKey(i => i.InvestorId);

            builder.Property(i => i.Address).IsRequired().HasMaxLength(500);
            builder.Property(i => i.RiskAppetite).HasConversion<string>().HasMaxLength(15);
            builder.Property(i => i.InvestorType).HasConversion<string>().HasMaxLength(15);
            builder.Property(i => i.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(i => i.UserId).IsUnique();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}