using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Features.Common;

namespace CashFlowSA.Application.Features.Funding.Common
{
    /// <summary>
    /// SRS 5.5 Auction model: resolves the winning bid once a campaign's
    /// FundingDeadline has passed. Highest BidAmount wins.
    ///
    /// ASSUMPTION: a winning bid does not need to equal campaign.TargetAmount --
    /// unlike SingleInvestor funding, an auction accepts whatever the highest
    /// bid is, even if it's less than the full target. If your SRS actually
    /// requires the winning bid to fully cover TargetAmount, add that check here.
    ///
    /// This is pure business logic with no knowledge of timers or hosting --
    /// it just needs an IApplicationDbContext. The actual "run this on a
    /// schedule" mechanics live in CashFlowSA.API.Services.AuctionCloseBackgroundService.
    /// </summary>
    public static class AuctionCloseService
    {
        public static async Task CloseExpiredAuctionsAsync(
            IApplicationDbContext context,
            CancellationToken cancellationToken)
        {
            await using var transaction = await context.Database
                .BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);

            var now = DateTime.UtcNow;

            var expiredCampaigns = await context.FundingCampaigns
                .Where(c => c.FundingModel == FundingModel.Auction
                    && (c.Status == CampaignStatus.Listed || c.Status == CampaignStatus.Funding)
                    && c.FundingDeadline.HasValue
                    && c.FundingDeadline.Value <= now)
                .OrderBy(c => c.CampaignId)
                .ToListAsync(cancellationToken);

            foreach (var campaign in expiredCampaigns)
            {
                var activeBids = await context.AuctionBids
                    .Where(b => b.CampaignId == campaign.CampaignId && b.Status == BidStatus.Active)
                    .ToListAsync(cancellationToken);

                if (activeBids.Count == 0)
                {
                    // No bids at all -- nothing to resolve. What should happen to an
                    // auction that closes with zero bids (Expired status? stays open?)
                    // isn't defined anywhere yet -- leaving the campaign untouched for now.
                    continue;
                }

                var winningBid = activeBids.OrderByDescending(b => b.BidAmount).First();

                foreach (var bid in activeBids)
                {
                    bid.Status = bid.BidId == winningBid.BidId ? BidStatus.Winning : BidStatus.Outbid;
                }

                // The winning bid becomes a real investment only after its investor
                // wallet is successfully debited. This keeps the wallet ledger and
                // campaign funding atomic.
                await InvestorWalletDebit.DebitAsync(
                    context,
                    winningBid.InvestorId,
                    winningBid.BidAmount,
                    campaign.CampaignId,
                    cancellationToken);

                var investment = new CashFlowSA.Domain.Models.Investment
                {
                    InvestmentId = Guid.NewGuid(),
                    CampaignId = campaign.CampaignId,
                    InvestorId = winningBid.InvestorId,
                    Amount = winningBid.BidAmount,
                    Status = InvestmentStatus.Committed,
                    InvestedAt = DateTime.UtcNow
                };

                context.Investments.Add(investment);

                campaign.FundedAmount = winningBid.BidAmount;
                campaign.Status = CampaignStatus.Funded;

                // Credit the SME with the actual winning bid amount.
                // Do not pass campaign.FundedAmount implicitly because the shared
                // helper now requires the amount being credited explicitly.
                await SmeFundingCredit.CreditSmeWalletAsync(
                    context,
                    campaign,
                    winningBid.BidAmount,
                    cancellationToken);
            }

            if (expiredCampaigns.Count > 0)
            {
                try
                {
                    await context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw new ConflictException(
                        "An auction campaign or investor wallet changed during auction close. The operation was rolled back.");
                }
            }
            else
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
    }
}