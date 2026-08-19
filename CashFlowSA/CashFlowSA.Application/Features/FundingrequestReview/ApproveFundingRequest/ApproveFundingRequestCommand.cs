using MediatR;

namespace CashFlowSA.Application.Features.FundingRequestReview.ApproveFundingRequest
{
    public class ApproveFundingRequestCommand : IRequest<Guid>
    {
        public Guid FundingRequestId { get; set; }
        public Guid ReviewerId { get; set; }

        // Required for SingleInvestor/Fractional campaigns, ignored for Auction
        // (auction rate is derived per-bid from AuctionBid.ProposedReturnRate).
        public decimal? ExpectedReturnRate { get; set; }

        // How long the listing stays open for investor commitments. Defaults to
        // 14 days from listing if the analyst doesn't set one.
        public DateTime? FundingDeadline { get; set; }
    }
}
