namespace CashFlowSA.Models.RiskAssessment
{
    public class AIExplanation
    {
        public Guid AIExplanationId { get; set; }

        public Guid RiskAssessmentId { get; set; }

        public string ExplanationText { get; set; } = string.Empty;

        public string InvestmentSummary { get; set; } = string.Empty;

        public string ModelUsed { get; set; } = string.Empty;

        // SRS 5.11 AC: if the AI service is unavailable, the rules-based score/grade
        // must still display. This flag lets the UI show "explanation unavailable"
        // instead of blocking the listing.
        public bool IsAvailable { get; set; } = true;

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//Plain-language, OpenAI-generated explanation of a RiskAssessment for investors.
//Generated as an independent async step after the rules-based score exists.
