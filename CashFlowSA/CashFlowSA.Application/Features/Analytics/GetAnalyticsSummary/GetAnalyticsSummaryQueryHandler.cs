using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Analytics.GetAnalyticsSummary;

public sealed class GetAnalyticsSummaryQueryHandler
    : IRequestHandler<GetAnalyticsSummaryQuery, AnalyticsSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetAnalyticsSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AnalyticsSummaryDto> Handle(
        GetAnalyticsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var campaigns = _context.FundingCampaigns.AsNoTracking();

        var totalCampaignCount = await campaigns
            .CountAsync(c => c.Status != CampaignStatus.Draft, cancellationToken);

        var successfulCampaignCount = await campaigns
            .CountAsync(c => c.Status == CampaignStatus.Funded || c.Status == CampaignStatus.Settled, cancellationToken);

        var activeCampaignCount = await campaigns
            .CountAsync(c => c.Status == CampaignStatus.Listed || c.Status == CampaignStatus.Funding, cancellationToken);

        var totalInvestorPrincipal = await _context.ReturnDistributions
            .AsNoTracking()
            .SumAsync(r => (decimal?)r.PrincipalAmount, cancellationToken) ?? 0m;

        var totalInvestorReturns = await _context.ReturnDistributions
            .AsNoTracking()
            .SumAsync(r => (decimal?)r.ReturnAmount, cancellationToken) ?? 0m;

        var campaignSuccessRate = totalCampaignCount == 0
            ? 0m
            : Math.Round(successfulCampaignCount * 100m / totalCampaignCount, 2);

        var investorRoi = totalInvestorPrincipal == 0m
            ? 0m
            : Math.Round(totalInvestorReturns * 100m / totalInvestorPrincipal, 2);

        return new AnalyticsSummaryDto
        {
            CampaignSuccessRate = campaignSuccessRate,
            InvestorRoi = investorRoi,
            ActiveCampaignCount = activeCampaignCount,
            TotalCampaignCount = totalCampaignCount,
            SuccessfulCampaignCount = successfulCampaignCount,
            TotalInvestorPrincipal = totalInvestorPrincipal,
            TotalInvestorReturns = totalInvestorReturns
        };
    }
}
