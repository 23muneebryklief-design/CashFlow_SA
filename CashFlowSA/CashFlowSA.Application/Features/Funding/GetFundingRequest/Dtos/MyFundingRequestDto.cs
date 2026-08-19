using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Funding.GetMyFundingRequests.Dtos
{
    public class MyFundingRequestDto
    {
        public Guid FundingRequestId { get; set; }
        public Guid InvoiceId { get; set; }
        public decimal RequestedAmount { get; set; }
        public FundingRequestStatus Status { get; set; }
        public string CurrentStage { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewDate { get; set; }
        public Guid? ReviewerId { get; set; }
        public string? ReviewNotes { get; set; }
        public Guid? CampaignId { get; set; }
        public CampaignStatus? CampaignStatus { get; set; }
    }
}
