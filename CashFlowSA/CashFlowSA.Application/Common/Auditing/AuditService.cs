using System.Text.Json;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Common.Auditing;

public interface IAuditService
{
    Task RecordAsync(
        AuditAction action,
        string entityType,
        Guid entityId,
        object? oldValue = null,
        object? newValue = null,
        CancellationToken cancellationToken = default);
}

public sealed class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task RecordAsync(
        AuditAction action,
        string entityType,
        Guid entityId,
        object? oldValue = null,
        object? newValue = null,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            return;

        _context.AuditLogs.Add(new AuditLog
        {
            AuditLogId = Guid.NewGuid(),
            UserId = userId.Value,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValue = oldValue is null ? null : JsonSerializer.Serialize(oldValue),
            NewValue = newValue is null ? null : JsonSerializer.Serialize(newValue),
            IPAddress = _currentUserService.IpAddress ?? string.Empty,
            Timestamp = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
