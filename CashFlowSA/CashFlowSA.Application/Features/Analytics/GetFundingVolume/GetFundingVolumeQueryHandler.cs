using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Analytics.GetFundingVolume
{
    // SRS 5.12: funding volume reporting, aggregated from FundingCampaign/Settlement.
    public class GetFundingVolumeQueryHandler : IRequestHandler<GetFundingVolumeQuery, FundingVolumeDto>
    {
        private readonly IApplicationDbContext _context;

        public GetFundingVolumeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<FundingVolumeDto> Handle(GetFundingVolumeQuery request, CancellationToken cancellationToken)
        {
            // Funding-volume analytics should represent campaigns that reached
            // the marketplace lifecycle, not draft campaigns that were never listed.
            var campaignsQuery = _context.FundingCampaigns
                .AsNoTracking()
                .Where(c => c.Status != CashFlowSA.Domain.Models.Enums.CampaignStatus.Draft);

            if (request.FromDate.HasValue)
            {
                var fromDate = request.FromDate.Value.Date;
                campaignsQuery = campaignsQuery.Where(c => c.CreatedAt >= fromDate);
            }

            if (request.ToDate.HasValue)
            {
                // Treat toDate as an inclusive calendar date.
                var toDateExclusive = request.ToDate.Value.Date.AddDays(1);
                campaignsQuery = campaignsQuery.Where(c => c.CreatedAt < toDateExclusive);
            }

            var totalCampaigns = await campaignsQuery.CountAsync(cancellationToken);
            var totalTargetAmount = await campaignsQuery
                .Select(c => (decimal?)c.TargetAmount)
                .SumAsync(cancellationToken) ?? 0m;
            var totalFundedAmount = await campaignsQuery
                .Select(c => (decimal?)c.FundedAmount)
                .SumAsync(cancellationToken) ?? 0m;

            var averageFundingPercentage = await campaignsQuery
                .Where(c => c.TargetAmount > 0)
                .Select(c => (decimal?)((c.FundedAmount / c.TargetAmount) * 100m))
                .AverageAsync(cancellationToken) ?? 0m;

            // Only completed settlements count toward delivered funding/settlement
            // analytics. Pending and failed records must not inflate the metric.
            var settlementsQuery = _context.Settlements
                .AsNoTracking()
                .Where(s => s.Status == CashFlowSA.Domain.Models.Enums.SettlementStatus.Completed);

            if (request.FromDate.HasValue)
            {
                var fromDate = request.FromDate.Value.Date;
                settlementsQuery = settlementsQuery.Where(s => s.SettlementDate >= fromDate);
            }

            if (request.ToDate.HasValue)
            {
                var toDateExclusive = request.ToDate.Value.Date.AddDays(1);
                settlementsQuery = settlementsQuery.Where(s => s.SettlementDate < toDateExclusive);
            }

            var totalSettledAmount = await settlementsQuery
                .Select(s => (decimal?)s.SettledAmount)
                .SumAsync(cancellationToken) ?? 0m;

            return new FundingVolumeDto
            {
                TotalCampaigns = totalCampaigns,
                TotalTargetAmount = totalTargetAmount,
                TotalFundedAmount = totalFundedAmount,
                TotalSettledAmount = totalSettledAmount,
                AverageFundingPercentage = averageFundingPercentage
            };
        }
    }
}
