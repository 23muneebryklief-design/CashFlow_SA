using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.FundingRequestReview.Dtos
{
    // Everything a Credit Analyst needs on the review queue without drilling
    // into the invoice separately -- includes the RiskAssessment produced
    // during invoice approval, since that's what the analyst is underwriting against.
    public class FundingRequestForReviewDto
    {
        public Guid FundingRequestId { get; set; }
        public Guid InvoiceId { get; set; }
        public Guid SMEId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public string DebtorName { get; set; } = string.Empty;
        public decimal InvoiceAmount { get; set; }
        public DateTime DueDate { get; set; }
        public decimal RequestedAmount { get; set; }
        public FundingModel FundingModel { get; set; }
        public FundingRequestStatus Status { get; set; }
        public DateTime SubmittedAt { get; set; }

        // Null if invoice approval's risk scoring somehow never ran -- the
        // approve endpoint blocks on this being present, but the queue should
        // still surface the gap visually rather than 500ing on load.
        public decimal? RiskScore { get; set; }
        public RiskGrade? RiskGrade { get; set; }
    }
}
