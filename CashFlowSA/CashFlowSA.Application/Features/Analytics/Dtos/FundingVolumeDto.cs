namespace CashFlowSA.Application.Features.Analytics.GetFundingVolume
{
    public class FundingVolumeDto
    {
        public int TotalCampaigns { get; set; }
        public decimal TotalTargetAmount { get; set; }
        public decimal TotalFundedAmount { get; set; }
        public decimal TotalSettledAmount { get; set; }
        public decimal AverageFundingPercentage { get; set; }
    }
}
