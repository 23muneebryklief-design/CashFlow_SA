using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Features.Funding.Common;

namespace CashFlowSA.API.Services
{
    /// <summary>
    /// Wakes up every minute and checks for Auction campaigns whose
    /// FundingDeadline has passed, resolving the winner via AuctionCloseService.
    ///
    /// This is a singleton that lives for the app's entire lifetime -- unlike
    /// every handler in this project, it can't just ask for IApplicationDbContext
    /// in its constructor (that's registered as scoped, one per HTTP request).
    /// Instead it asks for IServiceScopeFactory and manually creates a fresh
    /// scope on every tick, gets a fresh IApplicationDbContext out of that scope,
    /// does the work, then lets the scope (and its DbContext) get disposed --
    /// the same lifecycle ASP.NET Core normally handles automatically per-request.
    /// </summary>
    public class AuctionCloseBackgroundService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuctionCloseBackgroundService> _logger;

        public AuctionCloseBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<AuctionCloseBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Interval);

            while (!stoppingToken.IsCancellationRequested
                   && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                    await AuctionCloseService.CloseExpiredAuctionsAsync(context, stoppingToken);
                }
                catch (Exception ex)
                {
                    // A single failed tick must never crash the whole background
                    // service -- log it and try again on the next tick instead.
                    _logger.LogError(ex, "Error while closing expired auctions.");
                }
            }
        }
    }
}