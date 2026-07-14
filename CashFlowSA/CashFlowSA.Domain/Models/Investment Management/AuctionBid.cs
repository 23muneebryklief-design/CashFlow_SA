using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class AuctionBid : BaseEntity
    {
        public Guid BidId { get; set; }

        public Guid CampaignId { get; set; }

        public Guid InvestorId { get; set; }

        public decimal BidAmount { get; set; }

        // The "best terms" a bidder is offering, per SRS 5.5 Auction Funding row
        public decimal ProposedReturnRate { get; set; }

        public BidStatus Status { get; set; } = BidStatus.Active;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//A single bid in Auction Funding. SRS 5.5 AC: only the highest valid bid at
//auction close is accepted; bids submitted after close must be marked Late
//and rejected, never treated as the winner.
