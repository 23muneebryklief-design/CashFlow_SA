using MediatR;

namespace CashFlowSA.Application.Features.Analytics.GetSmeFundingHistory;

public sealed class GetSmeFundingHistoryQuery : IRequest<IReadOnlyList<SmeFundingHistoryDto>>
{
    public Guid? SmeId { get; init; }
}

public sealed class SmeFundingHistoryDto
{
    public Guid CampaignId { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal FundedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string FundingModel { get; set; } = string.Empty;
    public int TenorDays { get; set; }
    public DateTime? ListedAt { get; set; }
    public DateTime? FundingDeadline { get; set; }
}
