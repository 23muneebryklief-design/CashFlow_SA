using CashFlowSA.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Data
{
    public class CashFlowDbContext : DbContext
    {
        public CashFlowDbContext(DbContextOptions<CashFlowDbContext> options) : base(options)
        {
        }       
        public DbSet<User>Users { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }
        public DbSet<SME> SMES { get; set; }
        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }
    }
}
