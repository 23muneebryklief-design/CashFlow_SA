using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class MarketplaceListing : BaseEntity
    {
        public Guid ListingId { get; set; }

        public Guid CampaignId { get; set; }

        public RiskGrade RiskGrade { get; set; }

        // Denormalized from RiskAssessment so investors can filter/sort listings
        // (SRS 5.4) without joining across modules on every marketplace query.
        public decimal RiskScore { get; set; }

        public IndustryType Industry { get; set; }

        public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

        // SRS 5.4 AC: a fully funded listing is automatically removed from
        // the active browsing list. Set false rather than deleting the row,
        // so history/audit still has it.
        public bool IsActive { get; set; } = true;
    }
}

//Purpose:

//The browsable, filterable projection of a FundingCampaign shown to investors
//on the marketplace (SRS 5.4). One-to-one with FundingCampaign.
