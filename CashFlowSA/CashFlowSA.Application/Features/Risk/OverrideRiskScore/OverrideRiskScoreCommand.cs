using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.Risk.OverrideRiskScore;

public sealed class OverrideRiskScoreCommand : IRequest<Unit>
{
    public Guid InvoiceId { get; init; }
    public decimal RiskScore { get; init; }
    public RiskGrade RiskGrade { get; init; }
    public string Justification { get; init; } = string.Empty;
}
