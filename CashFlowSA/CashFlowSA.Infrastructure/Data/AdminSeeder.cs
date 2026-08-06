using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CashFlowSA.Infrastructure.Data
{
    // Runs once at startup. Creates a single Admin user from config if one
    // doesn't already exist -- there's no public registration endpoint for
    // Admin (intentionally, since /register/admin sitting open on the API
    // would let anyone create one), so this is the only way to get the
    // first admin account into the database.
    public static class AdminSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<CashFlowDbContext>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("AdminSeeder");

            var superAdminExists = await context.Users
                .AnyAsync(u => u.Role == UsersRoles.SuperAdmin);

            if (superAdminExists)
                return;

            var email = config["AdminSeed:Email"] ?? "admin@cashflowsa.co.za";
            var password = config["AdminSeed:Password"] ?? "ChangeMe123!";

            var admin = new User
            {
                UserId = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "User",
                Email = email,
                PhoneNumber = "0000000000",
                Role = UsersRoles.SuperAdmin,
                Status = AccountStatus.Active
            };

            var passwordHasher = new PasswordHasher<User>();
            admin.PasswordHash = passwordHasher.HashPassword(admin, password);

            context.Users.Add(admin);
            await context.SaveChangesAsync();

            logger.LogWarning(
                "Seeded default SuperAdmin account -- Email: {Email}, Password: {Password}. " +
                "This account can create additional Admin accounts from the admin dashboard.",
                email, password);
        }
    }
}
