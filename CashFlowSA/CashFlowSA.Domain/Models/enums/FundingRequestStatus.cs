namespace CashFlowSA.Domain.Models.Enums
{
    // SRS 3.3 / 3.1: a funding request must be analyst-approved before it becomes a listing
    public enum FundingRequestStatus
    {
        Pending=0,
        UnderReview=1,
        Approved=2,
        Rejected=3
    }
}
