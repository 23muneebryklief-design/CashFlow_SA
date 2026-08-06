using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Audit.GetAuditLogs;
using CashFlowSA.Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    // Auditor-only, read-only, append-only log access per SRS 5.8.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Auditor")]
    public class AuditController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuditController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] Guid? userId,
            [FromQuery] AuditAction? action,
            [FromQuery] string? entityType,
            CancellationToken cancellationToken)
        {
            var query = new GetAuditLogsQuery
            {
                UserId = userId,
                Action = action,
                EntityType = entityType
            };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}
