namespace CashFlowSA.Models.WalletFinancialSimulation
{
    public class ReturnDistribution
    {
        public Guid ReturnDistributionId { get; set; }

        public Guid SettlementId { get; set; }

        public Guid InvestmentId { get; set; }

        public Guid InvestorId { get; set; }

        public decimal PrincipalAmount { get; set; }

        public decimal ReturnAmount { get; set; }

        public DateTime DistributedAt { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//One row per investor per settled campaign, splitting the Settlement
//proportionally across all Investments in that campaign (SRS section 4,
//step 11). Feeds InvestorPortfolio.TotalReturned and analytics ROI figures.
