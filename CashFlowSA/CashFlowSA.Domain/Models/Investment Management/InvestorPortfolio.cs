namespace CashFlowSA.Domain.Models
{
    public class InvestorPortfolio : BaseEntity
    {
        public Guid PortfolioId { get; set; }

        public Guid InvestorId { get; set; }

        public decimal TotalCommitted { get; set; } = 0;

        public decimal TotalFunded { get; set; } = 0;

        public decimal TotalReturned { get; set; } = 0;

        public int ActiveInvestmentsCount { get; set; } = 0;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//Materialized, one-per-investor snapshot backing the portfolio dashboard
//(SRS 3.2 AC: must accurately reflect committed/funded/returned amounts
//at all times) - recalculated whenever an Investment or ReturnDistribution changes.
