namespace CashFlowSA.Models.enums
{
    // Matches SRS 3.1 AC: Draft -> Listed -> Funding -> Funded -> Settled
    public enum CampaignStatus
    {
        Draft=0,
        Listed=1,
        Funding=2,
        Funded=3,
        Settled=4,
        Expired=5
    }
}
