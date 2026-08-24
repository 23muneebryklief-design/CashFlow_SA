using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Marketplace.GetListings
{
    public class GetListingsQueryHandler : IRequestHandler<GetListingsQuery, List<ListingSummaryDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetListingsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ListingSummaryDto>> Handle(GetListingsQuery request, CancellationToken cancellationToken)
        {
            // SRS 5.4 AC: fully funded listings are marked IsActive = false rather
            // than deleted, so only active ones show up in the browsable list.
            var query = _context.MarketplaceListings
                .Where(l => l.IsActive)
                .Join(_context.FundingCampaigns,
                    l => l.CampaignId, c => c.CampaignId,
                    (l, c) => new { Listing = l, Campaign = c })
                .AsQueryable();

            if (request.RiskGrade.HasValue)
                query = query.Where(x => x.Listing.RiskGrade == request.RiskGrade.Value);

            if (request.Industry.HasValue)
                query = query.Where(x => x.Listing.Industry == request.Industry.Value);

            if (request.MinAmount.HasValue)
                query = query.Where(x => x.Campaign.TargetAmount >= request.MinAmount.Value);

            if (request.MaxAmount.HasValue)
                query = query.Where(x => x.Campaign.TargetAmount <= request.MaxAmount.Value);

            if (request.MinTenorDays.HasValue)
                query = query.Where(x => x.Campaign.TenorDays >= request.MinTenorDays.Value);

            if (request.MaxTenorDays.HasValue)
                query = query.Where(x => x.Campaign.TenorDays <= request.MaxTenorDays.Value);

            return await query
                .OrderByDescending(x => x.Listing.PublishedAt)
                .Select(x => new ListingSummaryDto
                {
                    ListingId = x.Listing.ListingId,
                    CampaignId = x.Listing.CampaignId,
                    RiskGrade = x.Listing.RiskGrade,
                    RiskScore = x.Listing.RiskScore,
                    Industry = x.Listing.Industry,
                    TargetAmount = x.Campaign.TargetAmount,
                    FundedAmount = x.Campaign.FundedAmount,
                    TenorDays = x.Campaign.TenorDays,
                    PublishedAt = x.Listing.PublishedAt
                })
                .ToListAsync(cancellationToken);
        }
    }
}
