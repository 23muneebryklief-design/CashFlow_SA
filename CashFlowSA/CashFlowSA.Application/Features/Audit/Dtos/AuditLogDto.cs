using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Audit.GetAuditLogs
{
    public class AuditLogDto
    {
        public Guid AuditLogId { get; set; }
        public Guid UserId { get; set; }
        public AuditAction Action { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string IPAddress { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
