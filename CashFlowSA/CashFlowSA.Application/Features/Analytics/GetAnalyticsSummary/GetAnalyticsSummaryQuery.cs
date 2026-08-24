using MediatR;

namespace CashFlowSA.Application.Features.Analytics.GetAnalyticsSummary;

public sealed class GetAnalyticsSummaryQuery : IRequest<AnalyticsSummaryDto>
{
}

public sealed class AnalyticsSummaryDto
{
    public decimal CampaignSuccessRate { get; set; }
    public decimal InvestorRoi { get; set; }
    public int ActiveCampaignCount { get; set; }
    public int TotalCampaignCount { get; set; }
    public int SuccessfulCampaignCount { get; set; }
    public decimal TotalInvestorPrincipal { get; set; }
    public decimal TotalInvestorReturns { get; set; }
}
