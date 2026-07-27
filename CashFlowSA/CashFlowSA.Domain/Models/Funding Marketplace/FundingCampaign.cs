using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class FundingCampaign : BaseEntity
    {
        public Guid CampaignId { get; set; }

        public Guid FundingRequestId { get; set; }

        public Guid InvoiceId { get; set; }

        public Guid SMEId { get; set; }

        public FundingModel FundingModel { get; set; }

        public decimal TargetAmount { get; set; }

        // Fixed annualized/tenor return rate promised to investors on this
        // campaign, as a percentage (e.g. 12.50 = 12.5%). Only meaningful for
        // SingleInvestor and Fractional models -- Auction campaigns instead
        // derive their rate per-investor from AuctionBid.ProposedReturnRate.
        // Nullable because it's set at campaign-creation time (during
        // underwriting), not at the moment this entity is first scaffolded.
        public decimal? ExpectedReturnRate { get; set; }

        // Running total of committed investor funds; must never exceed TargetAmount,
        // even under concurrent commitments (SRS 5.5 AC - needs row-locking/concurrency
        // control at the repository/service layer, e.g. a concurrency token column).
        public decimal FundedAmount { get; set; } = 0;

        public int TenorDays { get; set; }

        public CampaignStatus Status { get; set; } = CampaignStatus.Draft;

        public DateTime? ListedAt { get; set; }

        public DateTime? FundingDeadline { get; set; }

        // EF Core concurrency token to prevent over-funding races on FundedAmount
        [System.ComponentModel.DataAnnotations.Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}

//Purpose:

//The core funding lifecycle entity: Draft -> Listed -> Funding -> Funded -> Settled
//(SRS 3.1 AC / section 4). Investments reference this, not the MarketplaceListing.