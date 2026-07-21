using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }
        DbSet<SME> SMEs { get; }
        DbSet<Investor> Investors { get; }
        DbSet<UserSession>UserSessions{get;}
        DbSet<KYCApplication> KYCApplications { get; }
        DbSet<KYCDocuments> KYCDocuments { get; }
        
        DbSet<Invoice> Invoices {get;}
        DbSet<InvoiceDocument>InvoiceDocuments {get;}

        DbSet<MarketplaceListing> MarketplaceListings { get; }
        DbSet<FundingCampaign> FundingCampaigns { get; }
        DbSet<FundingRequest> FundingRequests { get; }
        DbSet<AuctionBid> AuctionBids { get; }
        DbSet<Investment> Investments { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}