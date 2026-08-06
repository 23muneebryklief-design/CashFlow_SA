using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
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
            CancellationToken cancellationToken)
        {
            var sme = await context.SMEs
                .FirstOrDefaultAsync(s => s.SMEId == campaign.SMEId, cancellationToken);

            // Shouldn't happen given the FK, but a funding disbursement is not
            // the place to throw on a data anomaly -- skip silently, same as
            // the missing-wallet case below.
            if (sme is null)
                return;

            var wallet = await context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == sme.UserId, cancellationToken);

            // NOTE: same gap as Settlement's investor crediting -- if the SME
            // has no wallet yet, the credit is silently skipped rather than
            // treated as an error. Worth a hard failure instead once wallets
            // are guaranteed to exist for every SME at registration time.
            if (wallet is null)
                return;

            wallet.Balance += campaign.FundedAmount;

            context.WalletTransactions.Add(new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                Type = WalletTransactionType.Credit,
                Amount = campaign.FundedAmount,
                ReferenceType = "FundingCampaign",
                ReferenceId = campaign.CampaignId,
                Description = $"Funds disbursed for campaign {campaign.CampaignId}"
            });
        }
    }
}
