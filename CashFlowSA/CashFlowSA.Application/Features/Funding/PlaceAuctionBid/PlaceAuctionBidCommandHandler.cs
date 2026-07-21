using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Funding.PlaceAuctionBid
{
    // SRS 5.5 Auction model. NOTE: this only records the bid. Determining the
    // winning bid at auction close (SRS AC: "only the highest valid bid at close
    // is accepted") is a separate process -- likely a scheduled job that runs at
    // FundingCampaign.FundingDeadline -- and is NOT implemented here.
    public class PlaceAuctionBidCommandHandler : IRequestHandler<PlaceAuctionBidCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public PlaceAuctionBidCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(PlaceAuctionBidCommand request, CancellationToken cancellationToken)
        {
            var campaign = await _context.FundingCampaigns
                .FirstOrDefaultAsync(c => c.CampaignId == request.CampaignId, cancellationToken);

            if (campaign is null)
                throw new NotFoundException("Funding campaign not found.");

            if (campaign.FundingModel != FundingModel.Auction)
                throw new ConflictException("This campaign is not configured for auction funding.");

            // SRS AC: bids submitted after close must be marked Late and rejected,
            // never treated as a valid/winning bid.
            var isLate = campaign.FundingDeadline.HasValue && DateTime.UtcNow > campaign.FundingDeadline.Value;

            if (isLate)
                throw new ConflictException("The auction for this campaign has closed. Late bids are not accepted.");

            if (campaign.Status != CampaignStatus.Listed && campaign.Status != CampaignStatus.Funding)
                throw new ConflictException("This campaign is not open for bidding.");

            var bid = new AuctionBid
            {
                BidId = Guid.NewGuid(),
                CampaignId = campaign.CampaignId,
                InvestorId = request.InvestorId,
                BidAmount = request.BidAmount,
                ProposedReturnRate = request.ProposedReturnRate,
                Status = BidStatus.Active,
                SubmittedAt = DateTime.UtcNow
            };

            _context.AuctionBids.Add(bid);

            if (campaign.Status == CampaignStatus.Listed)
                campaign.Status = CampaignStatus.Funding;

            await _context.SaveChangesAsync(cancellationToken);

            return bid.BidId;
        }
    }
}
