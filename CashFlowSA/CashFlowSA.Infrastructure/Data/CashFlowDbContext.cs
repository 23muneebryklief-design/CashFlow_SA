using System.Text.Json;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using CashFlowSA.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Infrastructure.Data
{
    public class CashFlowDbContext : DbContext, IApplicationDbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public CashFlowDbContext(
            DbContextOptions<CashFlowDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<UserSession> UserSessions => Set<UserSession>();
        public DbSet<SME> SMEs => Set<SME>();
        public DbSet<Investor> Investors => Set<Investor>();
        public DbSet<InvestorPortfolio> InvestorPortfolios => Set<InvestorPortfolio>();
        public DbSet<IndividualInvestorProfile> IndividualInvestorProfiles { get; set; }
        public DbSet<InstitutionalInvestorProfile> InstitutionalInvestorProfiles { get; set; }
        public DbSet<CorporateInvestorProfile> CorporateInvestorProfiles { get; set; }
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceDocument> InvoiceDocuments => Set<InvoiceDocument>();
        public DbSet<OCRResult> OCRResults => Set<OCRResult>();
        public DbSet<KYCApplication> KYCApplications => Set<KYCApplication>();
        public DbSet<KYCDocuments> KYCDocuments => Set<KYCDocuments>();
        public DbSet<KYCReview> KYCReviews => Set<KYCReview>();
        public DbSet<FundingCampaign> FundingCampaigns => Set<FundingCampaign>();
        public DbSet<FundingRequest> FundingRequests => Set<FundingRequest>();
        public DbSet<MarketplaceListing> MarketplaceListings => Set<MarketplaceListing>();
        public DbSet<AuctionBid> AuctionBids => Set<AuctionBid>();
        public DbSet<Investment> Investments => Set<Investment>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
        public DbSet<Settlement> Settlements => Set<Settlement>();
        public DbSet<ReturnDistribution> ReturnDistributions => Set<ReturnDistribution>();
        public DbSet<UnderwritingReview> UnderwritingReviews => Set<UnderwritingReview>();
        public DbSet<RiskAssessment> RiskAssessments => Set<RiskAssessment>();
        public DbSet<AIExplanation> AIExplanations => Set<AIExplanation>();
        public DbSet<RiskScoreHistory> RiskScoreHistories => Set<RiskScoreHistory>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NotificationHistory> NotificationHistories => Set<NotificationHistory>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        public DbSet<GeneratedReport> GeneratedReports => Set<GeneratedReport>();

        public override int SaveChanges()
        {
            PrepareAuditEntries();
            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            PrepareAuditEntries();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            PrepareAuditEntries();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            PrepareAuditEntries();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void PrepareAuditEntries()
        {
            var auditEntries = ChangeTracker.Entries()
                .Where(entry => entry.Entity is not AuditLog)
                .Where(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            var currentUserId = _currentUserService.UserId;
            var ipAddress = _currentUserService.IpAddress ?? string.Empty;
            var now = DateTime.UtcNow;
            var logs = new List<AuditLog>();

            foreach (var entry in auditEntries)
            {
                var userId = ResolveAuditUserId(entry, currentUserId);
                // AuditLog.UserId is a required FK. If a true system/background operation has
                // no user context, there is no safe actor to attribute it to, so leave it out
                // rather than inventing an audit identity.
                if (!userId.HasValue)
                    continue;

                if (entry.Entity is BaseEntity entity)
                {
                    if (entry.State == EntityState.Added)
                    {
                        entity.CreatedAt = now;
                        entity.CreatedByUserId = userId;
                    }
                    else if (entry.State == EntityState.Modified)
                    {
                        entity.UpdatedAt = now;
                        entity.UpdatedByUserId = userId;
                    }
                }

                var entityId = GetEntityId(entry);
                if (!entityId.HasValue)
                    continue;

                logs.Add(new AuditLog
                {
                    AuditLogId = Guid.NewGuid(),
                    UserId = userId.Value,
                    Action = ResolveAuditAction(entry),
                    EntityType = entry.Metadata.ClrType.Name,
                    EntityId = entityId.Value,
                    OldValue = entry.State == EntityState.Added ? null : SerializeValues(entry, useOriginalValues: true),
                    NewValue = entry.State == EntityState.Deleted ? null : SerializeValues(entry, useOriginalValues: false),
                    IPAddress = ipAddress,
                    Timestamp = now
                });
            }

            if (logs.Count > 0)
                AuditLogs.AddRange(logs);
        }

        private static Guid? ResolveAuditUserId(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
            Guid? currentUserId)
        {
            if (currentUserId.HasValue)
                return currentUserId;

            // Login/registration happen before an authenticated principal exists.
            // Use the affected user's own UserId so those security events remain attributable.
            var userIdProperty = entry.Properties.FirstOrDefault(p =>
                string.Equals(p.Metadata.Name, "UserId", StringComparison.OrdinalIgnoreCase));

            if (userIdProperty?.CurrentValue is Guid userId && userId != Guid.Empty)
                return userId;

            if (entry.Entity is User user && user.UserId != Guid.Empty)
                return user.UserId;

            return null;
        }

        private static AuditAction ResolveAuditAction(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var entityName = entry.Metadata.ClrType.Name;

            if (entry.Entity is UserSession)
            {
                if (entry.State == EntityState.Added)
                    return AuditAction.LoggedIn;

                if (entry.State == EntityState.Modified)
                {
                    var oldLogout = entry.Property(nameof(UserSession.LogoutTimestamp)).OriginalValue;
                    var newLogout = entry.Property(nameof(UserSession.LogoutTimestamp)).CurrentValue;
                    if (oldLogout is null && newLogout is DateTime)
                        return AuditAction.LoggedOut;
                }
            }

            if (entry.Entity is RiskScoreHistory && entry.State == EntityState.Added)
                return AuditAction.RiskOverridden;

            if (entry.Entity is GeneratedReport && entry.State == EntityState.Added)
            {
                var reportType = entry.Property(nameof(GeneratedReport.ReportType)).CurrentValue?.ToString();
                if (string.Equals(reportType, ReportType.Audit.ToString(), StringComparison.Ordinal))
                    return AuditAction.AuditReportGenerated;
            }

            if (entry.Entity is User && entry.State == EntityState.Modified)
            {
                var oldStatus = entry.Property(nameof(User.Status)).OriginalValue?.ToString();
                var newStatus = entry.Property(nameof(User.Status)).CurrentValue?.ToString();

                if (!string.Equals(oldStatus, newStatus, StringComparison.Ordinal))
                {
                    if (string.Equals(newStatus, AccountStatus.Suspended.ToString(), StringComparison.Ordinal))
                        return AuditAction.UserSuspended;

                    if (string.Equals(oldStatus, AccountStatus.Suspended.ToString(), StringComparison.Ordinal) &&
                        string.Equals(newStatus, AccountStatus.Active.ToString(), StringComparison.Ordinal))
                        return AuditAction.UserReinstated;
                }
            }

            if (entry.State == EntityState.Added)
            {
                return entityName switch
                {
                    nameof(KYCApplication) => AuditAction.Submitted,
                    nameof(KYCDocuments) => AuditAction.UploadedDocument,
                    nameof(InvoiceDocument) => AuditAction.UploadedDocument,
                    nameof(Document) => AuditAction.UploadedDocument,
                    nameof(FundingRequest) => AuditAction.Submitted,
                    nameof(Investment) => AuditAction.Invested,
                    nameof(Settlement) => AuditAction.Settled,
                    nameof(Invoice) when HasStatus(entry, InvoiceStatus.Submitted) => AuditAction.UploadedInvoice,
                    nameof(FundingCampaign) when HasStatus(entry, CampaignStatus.Funded) => AuditAction.Funded,
                    nameof(FundingCampaign) when HasStatus(entry, CampaignStatus.Settled) => AuditAction.Settled,
                    _ => AuditAction.Created
                };
            }

            if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("Status") is not null)
            {
                var currentStatus = entry.Property("Status").CurrentValue?.ToString();

                return currentStatus switch
                {
                    "Verified" => AuditAction.Approved,
                    "Approved" => AuditAction.Approved,
                    "Rejected" => AuditAction.Rejected,
                    "Pending" => AuditAction.Submitted,
                    "Funded" => AuditAction.Funded,
                    "Settled" => AuditAction.Settled,
                    "Completed" => AuditAction.Settled,

                    _ => AuditAction.Updated
                };
            }

            return entry.State switch
            {
                EntityState.Deleted => AuditAction.Deleted,
                _ => AuditAction.Updated
            };
        }

        private static bool HasStatus(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
            object expected)
        {
            var property = entry.Metadata.FindProperty("Status");
            return property is not null
                && string.Equals(entry.Property(property.Name).CurrentValue?.ToString(), expected.ToString(), StringComparison.Ordinal);
        }

        private static Guid? GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            var primaryKey = entry.Metadata.FindPrimaryKey();
            if (primaryKey is null || primaryKey.Properties.Count != 1)
                return null;

            var property = primaryKey.Properties[0];
            var value = entry.Property(property.Name).CurrentValue;

            if (value is Guid guid)
                return guid;

            return Guid.TryParse(value?.ToString(), out var parsed) ? parsed : null;
        }

        private static string SerializeValues(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
            bool useOriginalValues)
        {
            var values = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                if (IsSensitiveProperty(property.Metadata.Name))
                    continue;

                var value = useOriginalValues ? property.OriginalValue : property.CurrentValue;
                values[property.Metadata.Name] = value;
            }

            return JsonSerializer.Serialize(values);
        }

        private static bool IsSensitiveProperty(string propertyName)
        {
            var name = propertyName.Replace("_", string.Empty).ToLowerInvariant();
            return name.Contains("password")
                   || name.Contains("refreshtoken")
                   || name.Equals("token")
                   || name.Contains("secret")
                   || name.Contains("apikey");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
            modelBuilder.ApplyConfiguration(new SMEConfiguration());
            modelBuilder.ApplyConfiguration(new InvestorConfiguration());
            modelBuilder.ApplyConfiguration(new IndividualInvestorProfileConfiguration());
            modelBuilder.ApplyConfiguration(new InstitutionalInvestorProfileConfiguration());
            modelBuilder.ApplyConfiguration(new CorporateInvestorProfileConfiguration());
            modelBuilder.ApplyConfiguration(new InvestorPortfolioConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new OCRResultConfiguration());
            modelBuilder.ApplyConfiguration(new KYCApplicationConfiguration());
            modelBuilder.ApplyConfiguration(new KYCReviewConfiguration());
            modelBuilder.ApplyConfiguration(new KYCDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new FundingRequestConfiguration());
            modelBuilder.ApplyConfiguration(new FundingCampaignConfiguration());
            modelBuilder.ApplyConfiguration(new MarketplaceListingConfiguration());
            modelBuilder.ApplyConfiguration(new AuctionBidConfiguration());
            modelBuilder.ApplyConfiguration(new InvestmentConfiguration());
            modelBuilder.ApplyConfiguration(new WalletConfiguration());
            modelBuilder.ApplyConfiguration(new WalletTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new SettlementConfiguration());
            modelBuilder.ApplyConfiguration(new ReturnDistributionConfiguration());
            modelBuilder.ApplyConfiguration(new UnderwritingReviewConfiguration());
            modelBuilder.ApplyConfiguration(new RiskAssessmentConfiguration());
            modelBuilder.ApplyConfiguration(new AIExplanationConfiguration());
            modelBuilder.ApplyConfiguration(new RiskScoreHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new DocumentConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationConfiguration());
            modelBuilder.ApplyConfiguration(new NotificationHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
            modelBuilder.ApplyConfiguration(new GeneratedReportConfiguration());
        }
    }
}
