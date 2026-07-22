using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Marketplace.GetListingDetail
{
    public class GetListingDetailQueryHandler : IRequestHandler<GetListingDetailQuery, ListingDetailDto>
    {
        private readonly IApplicationDbContext _context;

        public GetListingDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ListingDetailDto> Handle(GetListingDetailQuery request, CancellationToken cancellationToken)
        {
            var listing = await _context.MarketplaceListings
                .FirstOrDefaultAsync(l => l.ListingId == request.ListingId, cancellationToken);

            if (listing is null)
                throw new NotFoundException("Listing not found.");

            var campaign = await _context.FundingCampaigns
                .FirstOrDefaultAsync(c => c.CampaignId == listing.CampaignId, cancellationToken);

            if (campaign is null)
                throw new NotFoundException("Funding campaign for this listing not found.");

            return new ListingDetailDto
            {
                ListingId = listing.ListingId,
                CampaignId = listing.CampaignId,
                RiskGrade = listing.RiskGrade,
                RiskScore = listing.RiskScore,
                Industry = listing.Industry,
                FundingModel = campaign.FundingModel,
                TargetAmount = campaign.TargetAmount,
                FundedAmount = campaign.FundedAmount,
                TenorDays = campaign.TenorDays,
                CampaignStatus = campaign.Status,
                FundingDeadline = campaign.FundingDeadline,
                PublishedAt = listing.PublishedAt
            };
        }
    }
}
