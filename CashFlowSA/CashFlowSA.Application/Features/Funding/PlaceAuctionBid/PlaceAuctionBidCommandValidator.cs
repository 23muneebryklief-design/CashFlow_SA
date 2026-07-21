using FluentValidation;

namespace CashFlowSA.Application.Features.Funding.PlaceAuctionBid
{
    public class PlaceAuctionBidCommandValidator : AbstractValidator<PlaceAuctionBidCommand>
    {
        public PlaceAuctionBidCommandValidator()
        {
            RuleFor(x => x.CampaignId).NotEmpty();
            RuleFor(x => x.InvestorId).NotEmpty();
            RuleFor(x => x.BidAmount).GreaterThan(0);
            RuleFor(x => x.ProposedReturnRate).GreaterThan(0);
        }
    }
}
