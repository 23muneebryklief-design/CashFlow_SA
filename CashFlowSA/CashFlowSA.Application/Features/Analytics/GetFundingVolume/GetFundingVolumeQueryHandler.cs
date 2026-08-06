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
            var campaignsQuery = _context.FundingCampaigns.AsQueryable();

            if (request.FromDate.HasValue)
                campaignsQuery = campaignsQuery.Where(c => c.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                campaignsQuery = campaignsQuery.Where(c => c.CreatedAt <= request.ToDate.Value);

            var campaigns = await campaignsQuery.ToListAsync(cancellationToken);

            var settlementsQuery = _context.Settlements.AsQueryable();

            if (request.FromDate.HasValue)
                settlementsQuery = settlementsQuery.Where(s => s.SettlementDate >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                settlementsQuery = settlementsQuery.Where(s => s.SettlementDate <= request.ToDate.Value);

            var totalSettled = await settlementsQuery.SumAsync(s => s.SettledAmount, cancellationToken);

            var averagePercentage = campaigns.Count > 0
                ? campaigns
                    .Where(c => c.TargetAmount > 0)
                    .Select(c => c.FundedAmount / c.TargetAmount * 100)
                    .DefaultIfEmpty(0)
                    .Average()
                : 0;

            return new FundingVolumeDto
            {
                TotalCampaigns = campaigns.Count,
                TotalTargetAmount = campaigns.Sum(c => c.TargetAmount),
                TotalFundedAmount = campaigns.Sum(c => c.FundedAmount),
                TotalSettledAmount = totalSettled,
                AverageFundingPercentage = averagePercentage
            };
        }
    }
}
