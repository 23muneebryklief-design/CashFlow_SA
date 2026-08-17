using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Common.Models
{
    public class AIRiskScoringResult
    {
        public decimal RiskScore { get; set; }

        public RiskGrade RiskGrade { get; set; }

        public string ScoringFactors { get; set; } = string.Empty;

        public string ExplanationText { get; set; } = string.Empty;

        public string InvestmentSummary { get; set; } = string.Empty;

        public string ModelUsed { get; set; } = string.Empty;
    }
}