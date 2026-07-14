namespace CashFlowSA.Domain.Models.Enums
{
    // SRS 5.5 Auction Funding: only the highest valid bid at close wins; late bids are rejected
    public enum BidStatus
    {
        Active=0,
        Winning=1,
        Outbid=2,
        Rejected=3,
        Late=4
    }
}
