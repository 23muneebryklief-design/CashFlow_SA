using CashFlowSA.Domain.Models;

namespace CashFlowSA.Application.Common.Interfaces
{
    public interface IRiskExplanationService
    {
        Task<AIExplanation> GenerateExplanationAsync(
            RiskAssessment riskAssessment,
            CancellationToken cancellationToken = default);
    }
}