using CashFlowSA.Domain.Models;
using CashFlowSA.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using CashFlowSA.Application.Common.Interfaces;

namespace CashFlowSA.Infrastructure.Data
{
    public class CashFlowDbContext : DbContext, IApplicationDbContext
    {
        public CashFlowDbContext(DbContextOptions<CashFlowDbContext> options)
            : base(options)
        {
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
            modelBuilder.ApplyConfiguration(new SMEConfiguration());
            modelBuilder.ApplyConfiguration(new InvestorConfiguration());
            modelBuilder.ApplyConfiguration(new IndividualInvestorProfilesConfiguration());
            modelBuilder.ApplyConfiguration(new InstitutionalInvestorProfilesConfiguration());
            modelBuilder.ApplyConfiguration(new CorporateInvestorProfilesConfiguration());
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
