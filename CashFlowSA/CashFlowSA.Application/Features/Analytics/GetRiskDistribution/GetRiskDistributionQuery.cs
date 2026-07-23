using MediatR;

namespace CashFlowSA.Application.Features.Analytics.GetRiskDistribution
{
    public class GetRiskDistributionQuery : IRequest<List<RiskGradeCountDto>>
    {
    }
}
