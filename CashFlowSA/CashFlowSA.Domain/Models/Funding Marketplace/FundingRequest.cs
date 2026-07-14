using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class FundingRequest : BaseEntity
    {
        public Guid FundingRequestId { get; set; }

        public Guid InvoiceId { get; set; }

        public Guid SMEId { get; set; }

        public decimal RequestedAmount { get; set; }

        public FundingModel FundingModel { get; set; }

        public FundingRequestStatus Status { get; set; } = FundingRequestStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DecisionAt { get; set; }
    }
}

//Purpose:

//An SME's request to finance an approved invoice under a chosen funding model.
//Reviewed by a Credit Analyst (UnderwritingReview) before a MarketplaceListing
//and FundingCampaign are created. A rejected request stops here (SRS 3.3 AC).
