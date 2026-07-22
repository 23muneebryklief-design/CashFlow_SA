using MediatR;

namespace CashFlowSA.Application.Features.Funding.PlaceAuctionBid
{
    public class PlaceAuctionBidCommand : IRequest<Guid>
    {
        public Guid CampaignId { get; set; }
        public Guid InvestorId { get; set; }
        public decimal BidAmount { get; set; }
        public decimal ProposedReturnRate { get; set; }
    }
}
