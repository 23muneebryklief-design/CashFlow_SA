using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Audit.GenerateAuditReport;

public sealed class GenerateAuditReportQueryHandler : IRequestHandler<GenerateAuditReportQuery, GenerateAuditReportResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GenerateAuditReportQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<GenerateAuditReportResult> Handle(
        GenerateAuditReportQuery request,
        CancellationToken cancellationToken)
    {
        var auditorId = _currentUserService.UserId;
        if (!auditorId.HasValue)
            throw new AuthenticationFailedException("Authenticated Auditor context is required.");

        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (request.From.HasValue)
        {
            var from = request.From.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.From.Value, DateTimeKind.Utc)
                : request.From.Value.ToUniversalTime();
            query = query.Where(x => x.Timestamp >= from);
        }

        if (request.To.HasValue)
        {
            var to = request.To.Value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(request.To.Value, DateTimeKind.Utc)
                : request.To.Value.ToUniversalTime();
            query = query.Where(x => x.Timestamp <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            var entityType = request.EntityType.Trim();
            query = query.Where(x => x.EntityType == entityType);
        }

        if (request.EntityId.HasValue)
            query = query.Where(x => x.EntityId == request.EntityId.Value);

        var entries = await query
            .OrderByDescending(x => x.Timestamp)
            .Select(x => new CashFlowSA.Application.Features.Audit.GetAuditLogs.AuditLogDto
            {
                AuditLogId = x.AuditLogId,
                UserId = x.UserId,
                Action = x.Action,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                OldValue = x.OldValue,
                NewValue = x.NewValue,
                IPAddress = x.IPAddress,
                Timestamp = x.Timestamp
            })
            .ToListAsync(cancellationToken);

        var reportId = Guid.NewGuid();
        var generatedAt = DateTime.UtcNow;
        var reportName = $"Audit Report {generatedAt:yyyy-MM-dd HH:mm:ss} UTC";

        _context.GeneratedReports.Add(new GeneratedReport
        {
            ReportId = reportId,
            GeneratedByUserId = auditorId.Value,
            ReportName = reportName,
            ReportType = ReportType.Audit,
            GeneratedAt = generatedAt,
            FilePath = string.Empty,
            Description = BuildDescription(request, entries.Count)
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new GenerateAuditReportResult
        {
            ReportId = reportId,
            ReportName = reportName,
            GeneratedAt = generatedAt,
            From = request.From,
            To = request.To,
            EntityType = string.IsNullOrWhiteSpace(request.EntityType) ? null : request.EntityType.Trim(),
            EntityId = request.EntityId,
            TotalEntries = entries.Count,
            Entries = entries
        };
    }

    private static string BuildDescription(GenerateAuditReportQuery request, int count)
    {
        var filters = new List<string>();

        if (request.From.HasValue)
            filters.Add($"from={request.From.Value:O}");

        if (request.To.HasValue)
            filters.Add($"to={request.To.Value:O}");

        if (!string.IsNullOrWhiteSpace(request.EntityType))
            filters.Add($"entityType={request.EntityType.Trim()}");

        if (request.EntityId.HasValue)
            filters.Add($"entityId={request.EntityId.Value}");

        var filterText = filters.Count == 0 ? "no filters" : string.Join(", ", filters);
        return $"Audit report generated with {count} entries ({filterText}).";
    }
}
