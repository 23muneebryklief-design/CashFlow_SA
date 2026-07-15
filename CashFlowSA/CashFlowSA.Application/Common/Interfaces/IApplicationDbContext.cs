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
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}