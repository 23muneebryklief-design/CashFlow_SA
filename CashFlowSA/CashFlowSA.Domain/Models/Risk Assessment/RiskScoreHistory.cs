using CashFlowSA.Models.enums;

namespace CashFlowSA.Models.RiskAssessment
{
    public class RiskScoreHistory
    {
        public Guid RiskScoreHistoryId { get; set; }

        public Guid InvoiceId { get; set; }

        public decimal PreviousScore { get; set; }

        public RiskGrade PreviousGrade { get; set; }

        public decimal NewScore { get; set; }

        public RiskGrade NewGrade { get; set; }

        public Guid ChangedByUserId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//Audit trail for Credit Analyst overrides of an automated risk score.
//SRS 3.3 AC: any manual override must be logged with analyst identity,
//timestamp, and a mandatory justification note.
