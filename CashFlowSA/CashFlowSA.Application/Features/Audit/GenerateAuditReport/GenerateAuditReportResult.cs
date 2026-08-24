using CashFlowSA.Application.Features.Audit.GetAuditLogs;

namespace CashFlowSA.Application.Features.Audit.GenerateAuditReport;

public sealed class GenerateAuditReportResult
{
    public Guid ReportId { get; init; }
    public string ReportName { get; init; } = string.Empty;
    public DateTime GeneratedAt { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public int TotalEntries { get; init; }
    public IReadOnlyList<AuditLogDto> Entries { get; init; } = Array.Empty<AuditLogDto>();
}
