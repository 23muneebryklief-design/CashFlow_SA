using CashFlowSA.Models.enums;

namespace CashFlowSA.Models.RiskAssessment
{
    public class RiskAssessment
    {
        public Guid RiskAssessmentId { get; set; }

        public Guid InvoiceId { get; set; }

        // 0-100 numeric score from the rules-based engine
        public decimal RiskScore { get; set; }

        public RiskGrade RiskGrade { get; set; }

        // Short free-text summary of which rules/factors drove the score
        // (e.g. debtor payment history, invoice tenor, industry risk)
        public string ScoringFactors { get; set; } = string.Empty;

        public bool IsOverridden { get; set; } = false;

        public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//The current, authoritative risk score/grade for an invoice (SRS 5.11).
//Every listed invoice must have one before it can appear on the marketplace.
