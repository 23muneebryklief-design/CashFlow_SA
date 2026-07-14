using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class Investment : BaseEntity
    {
        public Guid InvestmentId { get; set; }

        public Guid CampaignId { get; set; }

        public Guid InvestorId { get; set; }

        public decimal Amount { get; set; }

        public InvestmentStatus Status { get; set; } = InvestmentStatus.Committed;

        public DateTime InvestedAt { get; set; } = DateTime.UtcNow;

        // Filled in once ReturnDistribution runs at settlement
        public decimal? ReturnAmount { get; set; }
    }
}

//Purpose:

//One investor's commitment against a FundingCampaign. For Fractional funding,
//multiple Investments can exist per campaign; their sum must never exceed
//FundingCampaign.TargetAmount (SRS 5.5 AC).
