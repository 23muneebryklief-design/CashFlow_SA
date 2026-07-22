using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class IndividualInvestorProfileConfiguration : IEntityTypeConfiguration<IndividualInvestorProfile>
    {
        public void Configure(EntityTypeBuilder<IndividualInvestorProfile> builder)
        {
            builder.HasKey(p => p.IndividualInvestorProfileId);

            builder.Property(p => p.IdNumber)
                .HasMaxLength(13)
                .IsRequired();

            builder.Property(p => p.TaxNumber)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(p => p.SalaryRange)
                .HasConversion<string>()
                .HasMaxLength(30);
        }
    }

    public class CorporateInvestorProfileConfiguration : IEntityTypeConfiguration<CorporateInvestorProfile>
    {
        public void Configure(EntityTypeBuilder<CorporateInvestorProfile> builder)
        {
            builder.HasKey(p => p.CorporateInvestorProfileId);

            builder.Property(p => p.CompanyName)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(p => p.CompanyRegistrationNumber)
                .HasMaxLength(12)
                .IsRequired();

            builder.Property(p => p.TaxNumber)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(p => p.AuthorizedRepresentativeName)
                .HasMaxLength(25)
                .IsRequired();

            builder.Property(p => p.AuthorizedRepresentativeIdNumber)
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(p => p.UltimateBeneficialOwnerName)
                .HasMaxLength(25)
                .IsRequired();
        }
    }

    public class InstitutionalInvestorProfileConfiguration : IEntityTypeConfiguration<InstitutionalInvestorProfile>
    {
        public void Configure(EntityTypeBuilder<InstitutionalInvestorProfile> builder)
        {
            builder.HasKey(p => p.InstitutionalInvestorProfileId);

            builder.Property(p => p.InstitutionName)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(p => p.RegistrationNumber)
                .HasMaxLength(13)
                .IsRequired();

            builder.Property(p => p.FSCALicenseNumber)
                .HasMaxLength(5)
                .IsRequired();

            builder.Property(p => p.AuthorizedSignatoryName)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(p => p.AuthorizedSignatoryIdNumber)
                .HasMaxLength(10)
                .IsRequired();
        }
    }
}