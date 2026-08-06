using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class WalletTransaction : BaseEntity
    {
        public Guid TransactionId { get; set; }

        public Guid WalletId { get; set; }

        public WalletTransactionType Type { get; set; }

        public decimal Amount { get; set; }

        // e.g. "FundingCampaign", "Settlement" - lets one transaction table
        // record credits/debits from any source without a hard FK to each one
        public string ReferenceType { get; set; } = string.Empty;

        public Guid? ReferenceId { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}

//Purpose:

//Append-only ledger of every credit/debit against a Wallet (SRS 5.6 AC:
//wallet balance updates must correctly reflect funding credits and
//settlement debits). Balance on Wallet is a running total of these rows.
