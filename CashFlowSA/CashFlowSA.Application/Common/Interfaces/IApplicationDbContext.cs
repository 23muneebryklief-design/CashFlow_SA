using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<SME> SMEs { get; }
        DbSet<Investor> Investors { get; }
        DbSet<InvestorPortfolio> InvestorPortfolios { get; }
        DbSet<UserSession>UserSessions{get;}
        DbSet<KYCApplication> KYCApplications { get; }
        DbSet<KYCDocuments> KYCDocuments { get; }
        DbSet<KYCReview> KYCReviews { get; }
        
        DbSet<Invoice> Invoices {get;}
        DbSet<InvoiceDocument>InvoiceDocuments {get;}

        DbSet<MarketplaceListing> MarketplaceListings { get; }
        DbSet<FundingCampaign> FundingCampaigns { get; }
        DbSet<FundingRequest> FundingRequests { get; }
        DbSet<AuctionBid> AuctionBids { get; }
        DbSet<Investment> Investments { get; }

        DbSet<Wallet> Wallets { get; }
        DbSet<WalletTransaction> WalletTransactions { get; }
        DbSet<Settlement> Settlements { get; }
        DbSet<ReturnDistribution> ReturnDistributions { get; }
        DbSet<Notification> Notifications { get; }
        DbSet<NotificationHistory> NotificationHistories { get; }

        DbSet<AuditLog> AuditLogs { get; }
        DbSet<RiskAssessment> RiskAssessments { get; }
        DbSet<AIExplanation> AIExplanations { get; }

        Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade Database { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}