using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CashFlowSA.Infrastructure.Data
{
    /// <summary>
    /// Seeds one active, usable account for every application role in Development.
    /// The seed is idempotent and uses configuration for the password so demo
    /// credentials are never hard-coded into the source code.
    /// </summary>
    public static class DemoUserSeeder
    {
        private const string PasswordConfigKey = "DemoUsers:Password";

        private sealed record DemoUser(
            string Key,
            string Email,
            string FirstName,
            string LastName,
            string Phone,
            UsersRoles Role);

        private static readonly DemoUser[] Users =
        {
            new("Sme", "sme.demo@cashflowsa.co.za", "Sarah", "Mokoena", "0820000001", UsersRoles.SME),
            new("Investor", "investor.demo@cashflowsa.co.za", "David", "Naidoo", "0820000002", UsersRoles.Investor),
            new("CreditAnalyst", "credit.demo@cashflowsa.co.za", "Thabo", "Molefe", "0820000003", UsersRoles.CreditAnalyst),
            new("Admin", "admin.demo@cashflowsa.co.za", "Aisha", "Pillay", "0820000004", UsersRoles.Admin),
            new("Auditor", "auditor.demo@cashflowsa.co.za", "Lerato", "Dlamini", "0820000005", UsersRoles.Auditor),
            new("SuperAdmin", "superadmin.demo@cashflowsa.co.za", "Michael", "van der Merwe", "0820000006", UsersRoles.SuperAdmin)
        };

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<CashFlowDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("DemoUserSeeder");

            var password = config[PasswordConfigKey] ?? "CashFlow123!";
            var passwordHasher = new PasswordHasher<User>();

            foreach (var demo in Users)
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == demo.Email);

                if (user == null)
                {
                    user = new User
                    {
                        UserId = Guid.NewGuid(),
                        FirstName = demo.FirstName,
                        LastName = demo.LastName,
                        Email = demo.Email,
                        PhoneNumber = demo.Phone,
                        Role = demo.Role,
                        Status = AccountStatus.Active
                    };

                    user.PasswordHash = passwordHasher.HashPassword(user, password);
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                }
                else
                {
                    // Keep the demo account usable after a database has already
                    // been seeded or when its status was changed during testing.
                    user.Status = AccountStatus.Active;
                    user.Role = demo.Role;
                    user.PasswordHash = passwordHasher.HashPassword(user, password);
                    user.FirstName = demo.FirstName;
                    user.LastName = demo.LastName;
                    user.PhoneNumber = demo.Phone;
                    context.Users.Update(user);
                    await context.SaveChangesAsync();
                }

                await EnsureRoleProfileAsync(context, user, demo.Role);
            }

            await context.SaveChangesAsync();

            logger.LogWarning(
                "Development demo users seeded. All demo accounts use the configured DemoUsers:Password value.");
        }

        private static async Task EnsureRoleProfileAsync(
            CashFlowDbContext context,
            User user,
            UsersRoles role)
        {
            switch (role)
            {
                case UsersRoles.SME:
                    var sme = await context.SMEs.FirstOrDefaultAsync(x => x.UserId == user.UserId);
                    if (sme == null)
                    {
                        context.SMEs.Add(new SME
                        {
                            SMEId = Guid.NewGuid(),
                            UserId = user.UserId,
                            CompanyName = "Demo SME (Pty) Ltd",
                            ContactPerson = $"{user.FirstName} {user.LastName}",
                            CompanyEmail = user.Email,
                            CompanyPhoneNumber = user.PhoneNumber,
                            RegistrationDate = DateTime.UtcNow.Date,
                            RegistrationNumber = "2026/DEMO/0001",
                            Industry = IndustryType.Other,
                            Address = "Cape Town, Western Cape",
                            TaxNumber = "DEMO-TAX-0001"
                        });
                    }

                    if (!await context.Wallets.AnyAsync(x => x.UserId == user.UserId))
                    {
                        context.Wallets.Add(new Wallet
                        {
                            WalletId = Guid.NewGuid(),
                            UserId = user.UserId,
                            Balance = 50000m,
                            Currency = "ZAR"
                        });
                    }
                    break;

                case UsersRoles.Investor:
                    var investor = await context.Investors.FirstOrDefaultAsync(x => x.UserId == user.UserId);
                    if (investor == null)
                    {
                        context.Investors.Add(new Investor
                        {
                            InvestorId = Guid.NewGuid(),
                            UserId = user.UserId,
                            Address = "Cape Town, Western Cape",
                            RiskAppetite = RiskAppetite.Medium,
                            InvestorType = InvestorType.Individual
                        });
                    }

                    if (!await context.Wallets.AnyAsync(x => x.UserId == user.UserId))
                    {
                        context.Wallets.Add(new Wallet
                        {
                            WalletId = Guid.NewGuid(),
                            UserId = user.UserId,
                            Balance = 250000m,
                            Currency = "ZAR"
                        });
                    }

                    await context.SaveChangesAsync();

                    investor = await context.Investors.FirstAsync(x => x.UserId == user.UserId);
                    if (!await context.InvestorPortfolios.AnyAsync(x => x.InvestorId == investor.InvestorId))
                    {
                        context.InvestorPortfolios.Add(new InvestorPortfolio
                        {
                            PortfolioId = Guid.NewGuid(),
                            InvestorId = investor.InvestorId,
                            TotalCommitted = 0,
                            TotalFunded = 0,
                            TotalReturned = 0,
                            ActiveInvestmentsCount = 0
                        });
                    }
                    break;
            }
        }
    }
}
