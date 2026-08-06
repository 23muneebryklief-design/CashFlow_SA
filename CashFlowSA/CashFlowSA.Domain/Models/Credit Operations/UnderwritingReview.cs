using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class UnderwritingReview : BaseEntity
    {
        public Guid ReviewId { get; set; }

        public Guid FundingRequestId { get; set; }

        public Guid ReviewerId { get; set; }

        public UnderwritingDecision Decision { get; set; } = UnderwritingDecision.Pending;

        public string Notes { get; set; } = string.Empty;

        public string RiskJustification { get; set; } = string.Empty;

        public bool RiskScoreOverridden { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
    }
}