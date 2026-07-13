
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Data
{
    public class CashFlowDbContext : DbContext
    {
        public CashFlowDbContext(DbContextOptions<CashFlowDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
    }
}

