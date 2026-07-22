using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class Settlement : BaseEntity
    {
        public Guid SettlementId { get; set; }

        public Guid CampaignId { get; set; }

        public decimal SettledAmount { get; set; }

        public SettlementStatus Status { get; set; } = SettlementStatus.Pending;

        // "PayFast Sandbox" / "Ozow Sandbox" - always sandbox for this project (SRS 2.2)
        public string PaymentProvider { get; set; } = string.Empty;

        public string ReferenceNumber { get; set; } = string.Empty;

        public DateTime SettlementDate { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//Records the simulated debtor payment that closes out a FundingCampaign
//(SRS section 4, step 10). Triggers ReturnDistribution rows once Completed.
