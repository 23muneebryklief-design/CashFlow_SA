using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Analytics.GetRiskDistribution
{
    public class RiskGradeCountDto
    {
        public RiskGrade RiskGrade { get; set; }
        public int Count { get; set; }
    }
}
