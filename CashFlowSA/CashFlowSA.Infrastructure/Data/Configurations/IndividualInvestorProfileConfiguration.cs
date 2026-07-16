using CashFlowSA.Domain.Models.InvestorManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class IndividualInvestorProfileConfiguration : IEntityTypeConfiguration<IndividualInvestorProfile>
    {
        public void Configure(EntityTypeBuilder<IndividualInvestorProfile> builder)
        {
            builder.HasKey(p => p.IndividualInvestorProfileId);
            builder.Property(p => p. IdNumber).MaxLength(13);
            builder.Property(p => p. TaxNumber).MaxLength(10);
            
        }
    }
}