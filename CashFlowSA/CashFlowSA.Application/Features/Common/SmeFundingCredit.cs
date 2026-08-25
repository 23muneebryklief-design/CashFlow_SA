using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using CashFlowSA.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Funding.Common
{
    /// <summary>
    /// Shared by both commit handlers: the moment a FundingCampaign first
    /// reaches Funded status, the SME who raised it should be credited the
    /// full FundedAmount. Previously nothing did this -- money only ever
    /// flowed into investor wallets at Settlement, never into the SME's
    /// wallet at Funded. This closes that gap.
    ///
    /// Mirrors the investor-crediting pattern in TriggerSettlementCommandHandler:
    /// same "if wallet is missing, skip rather than throw" behavior, same
    /// WalletTransaction audit trail shape.
    /// </summary>
    internal static class SmeFundingCredit
    {
        public static async Task CreditSmeWalletAsync(
            IApplicationDbContext context,
            FundingCampaign campaign,
            decimal fundingAmount,
            CancellationToken cancellationToken)
        {
            var sme = await context.SMEs
                .FirstOrDefaultAsync(s => s.SMEId == campaign.SMEId, cancellationToken);

            if (sme is null)
                throw new ConflictException("SME profile not found; funding cannot be disbursed safely.");

            var wallet = await context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == sme.UserId, cancellationToken);

            if (wallet is null)
                throw new ConflictException("SME wallet not found; funding cannot be disbursed safely.");

            wallet.Balance += fundingAmount;

            context.WalletTransactions.Add(new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                Type = WalletTransactionType.Credit,
                Amount = fundingAmount,
                ReferenceType = "FundingCampaign",
                ReferenceId = campaign.CampaignId,
                Description = $"Funds received from funding commitment for campaign {campaign.CampaignId}"
            });
        }
    }
}
