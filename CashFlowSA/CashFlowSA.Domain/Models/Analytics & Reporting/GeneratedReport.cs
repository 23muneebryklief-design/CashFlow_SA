using CashFlowSA.Models.enums;
namespace CashFlowSA.Models.AnalyticsReporting
{
    public class GeneratedReport
    {
        public Guid ReportId { get; set; }

        public Guid GeneratedByUserId { get; set; }

        public string ReportName { get; set; } = string.Empty;

        public ReportType ReportType { get; set; } = ReportType.InvestorROI;

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public string FilePath { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
//Purpose

//Stores generated reports such as:

//Funding Volume Reports
//Investor ROI Reports
//SME Funding History Reports
//Risk Distribution Reports
//Audit Reports
//Platform Performance Reports