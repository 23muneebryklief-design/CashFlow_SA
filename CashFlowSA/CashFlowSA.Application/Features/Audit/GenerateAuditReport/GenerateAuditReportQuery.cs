using MediatR;

namespace CashFlowSA.Application.Features.Audit.GenerateAuditReport;

public sealed class GenerateAuditReportQuery : IRequest<GenerateAuditReportResult>
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
}
