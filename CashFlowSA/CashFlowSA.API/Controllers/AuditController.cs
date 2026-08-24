using CashFlowSA.Application.Features.Audit.GenerateAuditReport;
using CashFlowSA.Application.Features.Audit.GetAuditLogs;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlowSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Auditor")]
public sealed class AuditController : ControllerBase
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

    [HttpGet("report")]
    public async Task<IActionResult> GenerateReport(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        CancellationToken cancellationToken)
    {
        var query = new GenerateAuditReportQuery
        {
            From = from,
            To = to,
            EntityType = entityType,
            EntityId = entityId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
