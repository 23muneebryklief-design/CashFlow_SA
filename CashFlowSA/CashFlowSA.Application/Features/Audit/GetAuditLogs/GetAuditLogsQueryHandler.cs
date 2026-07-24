using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Audit.GetAuditLogs
{
    // SRS 5.8: Auditor-only, read-only, append-only log access.
    // Role enforcement (Auditor only) belongs at the controller via [Authorize],
    // not here -- this handler assumes it's only ever reached by an authorized caller.
    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAuditLogsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (request.UserId.HasValue)
                query = query.Where(a => a.UserId == request.UserId.Value);

            if (request.Action.HasValue)
                query = query.Where(a => a.Action == request.Action.Value);

            if (!string.IsNullOrEmpty(request.EntityType))
                query = query.Where(a => a.EntityType == request.EntityType);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Select(a => new AuditLogDto
                {
                    AuditLogId = a.AuditLogId,
                    UserId = a.UserId,
                    Action = a.Action,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    OldValue = a.OldValue,
                    NewValue = a.NewValue,
                    IPAddress = a.IPAddress,
                    Timestamp = a.Timestamp
                })
                .ToListAsync(cancellationToken);
        }
    }
}
