using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Marketplace.GetListings
{
    public class ListingSummaryDto
    {
        public Guid ListingId { get; set; }
        public Guid CampaignId { get; set; }
        public RiskGrade RiskGrade { get; set; }
        public decimal RiskScore { get; set; }
        public IndustryType Industry { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal FundedAmount { get; set; }
        public int TenorDays { get; set; }
        public DateTime PublishedAt { get; set; }
    }
}
