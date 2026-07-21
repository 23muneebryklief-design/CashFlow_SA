using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Funding.GetCampaignStatus
{
    public class CampaignStatusDto
    {
        public Guid CampaignId { get; set; }
        public CampaignStatus Status { get; set; }
        public FundingModel FundingModel { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal FundedAmount { get; set; }
        public DateTime? FundingDeadline { get; set; }
    }
}
