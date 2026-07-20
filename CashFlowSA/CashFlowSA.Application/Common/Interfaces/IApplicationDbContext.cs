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
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}