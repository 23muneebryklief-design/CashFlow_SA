using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Marketplace.GetListingDetail
{
    public class ListingDetailDto
    {
        public Guid ListingId { get; set; }
        public Guid CampaignId { get; set; }
        public RiskGrade RiskGrade { get; set; }
        public decimal RiskScore { get; set; }
        public IndustryType Industry { get; set; }
        public FundingModel FundingModel { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal FundedAmount { get; set; }
        public int TenorDays { get; set; }
        public CampaignStatus CampaignStatus { get; set; }
        public DateTime? FundingDeadline { get; set; }
        public DateTime PublishedAt { get; set; }

        // AI-generated plain-language explanation of the risk assessment (SRS 5.11).
        // Null/empty when no AIExplanation record exists yet; ExplanationAvailable
        // false when Ollama failed at approval time and a fallback was stored.
        public string? RiskExplanationText { get; set; }
        public string? InvestmentSummary { get; set; }
        public bool ExplanationAvailable { get; set; }
    }
}