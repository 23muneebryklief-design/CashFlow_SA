using CashFlowSA.Application.Features.Risk.OverrideRiskScore;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlowSA.API.Controllers;

[ApiController]
[Route("api/risk")]
[Authorize(Roles = "CreditAnalyst")]
public sealed class RiskController : ControllerBase
{
    private readonly IMediator _mediator;

    public RiskController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("override")]
    public async Task<IActionResult> Override(
        [FromBody] OverrideRiskScoreCommand command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);
        return Ok(new
        {
            message = "Risk assessment overridden successfully.",
            invoiceId = command.InvoiceId,
            riskScore = command.RiskScore,
            riskGrade = command.RiskGrade
        });
    }
}
