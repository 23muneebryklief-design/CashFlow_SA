using CashFlowSA.Application.Common.Models;
using CashFlowSA.Domain.Models;

namespace CashFlowSA.Application.Common.Interfaces
{
    public interface IRiskScoringService
    {
        Task<AIRiskScoringResult> CalculateRiskAsync(
            Invoice invoice,
            SME sme,
            CancellationToken cancellationToken = default);
    }
}