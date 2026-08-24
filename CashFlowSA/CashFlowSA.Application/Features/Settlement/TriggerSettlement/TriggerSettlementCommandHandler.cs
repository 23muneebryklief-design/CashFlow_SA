using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Notifications;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CashFlowSA.Application.Features.Settlement.TriggerSettlement
{
    // SRS section 4, steps 10-11: debtor payment closes out a FundingCampaign,
    // which then splits the payment across every investor proportional to
    // their share of the campaign, and credits each investor's wallet.
    //
    // ASSUMPTIONS (the SRS/Domain don't fully define these -- confirm before relying on them):
    // 1. No explicit interest-rate field exists anywhere on FundingCampaign/Invoice
    //    for single-investor/fractional models (AuctionBid has ProposedReturnRate,
    //    but only for the Auction model). This handler treats
    //    (SettledAmount - FundedAmount) as the total "profit" pool to distribute,
    //    split pro-rata by each Investment's share of FundedAmount. If your SRS
    //    defines a fixed/expected return rate elsewhere, this needs to change.
    // 2. SME wallet crediting when a campaign first becomes Funded is NOT
    //    implemented anywhere yet (including the Funding commands built earlier)
    //    -- this handler only credits investors at settlement time, not the SME
    //    at funding time. That's a separate gap.
    public class TriggerSettlementCommandHandler : IRequestHandler<TriggerSettlementCommand, Guid>
    {
        private readonly IApplicationDbContext _context;
        private readonly INotificationDispatcher _notifications;

        public TriggerSettlementCommandHandler(
            IApplicationDbContext context,
            INotificationDispatcher notifications)
        {
            _context = context;
            _notifications = notifications;
        }

        public async Task<Guid> Handle(TriggerSettlementCommand request, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var campaign = await _context.FundingCampaigns
                .FirstOrDefaultAsync(c => c.CampaignId == request.CampaignId, cancellationToken);

            if (campaign is null)
                throw new NotFoundException("Funding campaign not found.");

            if (campaign.Status != CampaignStatus.Funded)
                throw new ConflictException("Only fully Funded campaigns can be settled.");

            if (request.SettledAmount < campaign.FundedAmount)
                throw new ConflictException("Settlement amount cannot be less than the campaign's funded amount.");

            var existingSettlement = await _context.Settlements
                .AnyAsync(s => s.CampaignId == campaign.CampaignId, cancellationToken);

            if (existingSettlement)
                throw new ConflictException("This campaign has already been settled.");

            var settlement = new Domain.Models.Settlement
            {
                SettlementId = Guid.NewGuid(),
                CampaignId = campaign.CampaignId,
                SettledAmount = request.SettledAmount,
                Status = SettlementStatus.Completed,
                PaymentProvider = request.PaymentProvider,
                ReferenceNumber = request.ReferenceNumber,
                SettlementDate = DateTime.UtcNow
            };

            _context.Settlements.Add(settlement);

            var notificationRecipients = new List<(Guid UserId, decimal TotalCredit, decimal ReturnAmount)>();

            var investments = await _context.Investments
                .Where(i => i.CampaignId == campaign.CampaignId && i.Status == InvestmentStatus.Committed)
                .OrderBy(i => i.InvestorId)
                .ThenBy(i => i.InvestmentId)
                .ToListAsync(cancellationToken);

            // See ASSUMPTION 1 above.
            var totalReturnPool = Math.Max(0, request.SettledAmount - campaign.FundedAmount);

            foreach (var investment in investments)
            {
                var share = campaign.FundedAmount > 0 ? investment.Amount / campaign.FundedAmount : 0;
                var returnAmount = share * totalReturnPool;

                var distribution = new ReturnDistribution
                {
                    ReturnDistributionId = Guid.NewGuid(),
                    SettlementId = settlement.SettlementId,
                    InvestmentId = investment.InvestmentId,
                    InvestorId = investment.InvestorId,
                    PrincipalAmount = investment.Amount,
                    ReturnAmount = returnAmount,
                    DistributedAt = DateTime.UtcNow
                };

                _context.ReturnDistributions.Add(distribution);

                investment.ReturnAmount = returnAmount;
                investment.Status = InvestmentStatus.Returned;

                // Credit the investor's wallet with principal + return.
                var investor = await _context.Investors
                    .FirstOrDefaultAsync(inv => inv.InvestorId == investment.InvestorId, cancellationToken);

                if (investor is not null)
                {
                    var wallet = await _context.Wallets
                        .FirstOrDefaultAsync(w => w.UserId == investor.UserId, cancellationToken);

                    if (wallet is null)
                        throw new ConflictException("Investor wallet not found; settlement cannot be completed safely.");

                    {
                        var totalCredit = investment.Amount + returnAmount;
                        wallet.Balance += totalCredit;

                        _context.WalletTransactions.Add(new WalletTransaction
                        {
                            TransactionId = Guid.NewGuid(),
                            WalletId = wallet.WalletId,
                            Type = WalletTransactionType.Credit,
                            Amount = totalCredit,
                            ReferenceType = "Settlement",
                            ReferenceId = settlement.SettlementId,
                            Description = $"Principal + return for campaign {campaign.CampaignId}"
                        });

                        notificationRecipients.Add((investor.UserId, totalCredit, returnAmount));
                    }
                }
            }

            campaign.Status = CampaignStatus.Settled;

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictException(
                    "The campaign or an investor wallet changed during settlement. Please retry.");
            }

            foreach (var recipient in notificationRecipients)
            {
                await _notifications.DispatchAsync(
                    recipient.UserId,
                    NotificationEvent.SettlementCompleted,
                    "Settlement completed",
                    $"Settlement for campaign {campaign.CampaignId} credited R {recipient.TotalCredit:N2} to your wallet, including R {recipient.ReturnAmount:N2} in return.",
                    new[] { NotificationChannel.InApp },
                    cancellationToken);
            }

            return settlement.SettlementId;
        }
    }
}
