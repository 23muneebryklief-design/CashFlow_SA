namespace CashFlowSA.Application.Features.Investment.GetInvestorInvestments;

public sealed class InvestorInvestmentsDto
{
    public Guid InvestmentId { get; set; }
    public Guid CampaignId { get; set; }
    public string Industry { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime InvestedAt { get; set; }
    public int TenorDays { get; set; }
    public decimal? ReturnAmount { get; set; }
}
