using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CashFlowSA.Infrastructure.Data.Configurations
{
    public class SMEConfiguration : IEntityTypeConfiguration<SME>
    {
        public void Configure(EntityTypeBuilder<SME> builder)
        {
            builder.HasKey(s => s.SMEId);

            builder.Property(s => s.CompanyName).IsRequired().HasMaxLength(200);
            builder.Property(s => s.ContactPerson).IsRequired().HasMaxLength(200);
            builder.Property(s => s.CompanyEmail).IsRequired().HasMaxLength(256);
            builder.Property(s => s.CompanyPhoneNumber).HasMaxLength(30);
            builder.Property(s => s.RegistrationNumber).HasMaxLength(100);
            builder.Property(s => s.Address).HasMaxLength(500);
            builder.Property(s => s.TaxNumber).HasMaxLength(100);
            builder.Property(s => s.Industry).HasConversion<int>();
            builder.Property(s => s.RegistrationDate).HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(s => s.UserId);
            builder.HasIndex(s => s.CompanyEmail).IsUnique();
            builder.HasIndex(s => s.RegistrationNumber).IsUnique();

            builder.HasOne(s => s.User)
                .WithMany(u => u.SMEs)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.KYCApplications)
                .WithOne(k => k.SME)
                .HasForeignKey(k => k.SMEId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
