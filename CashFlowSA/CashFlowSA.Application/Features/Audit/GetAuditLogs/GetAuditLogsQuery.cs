using CashFlowSA.Domain.Models.Enums;
using MediatR;

namespace CashFlowSA.Application.Features.Audit.GetAuditLogs
{
    public class GetAuditLogsQuery : IRequest<List<AuditLogDto>>
    {
        // All optional -- SRS 5.8: Auditors get full unfiltered access by default,
        // these narrow the view down when supplied.
        public Guid? UserId { get; set; }
        public AuditAction? Action { get; set; }
        public string? EntityType { get; set; }
    }
}
