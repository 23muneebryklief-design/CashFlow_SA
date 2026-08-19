using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CashFlowSA.Application.Features.Investment.GetInvestorInvestments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashFlowSA.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Investor")]
public class InvestmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public InvestmentController(IMediator mediator) => _mediator = mediator;

    [HttpGet("investor/{investorId}")]
    public async Task<IActionResult> GetInvestorInvestments(
        Guid investorId,
        CancellationToken cancellationToken)
    {
        if (!TryGetProfileId(out var authenticatedInvestorId) || authenticatedInvestorId != investorId)
            return Forbid();

        var result = await _mediator.Send(
            new GetInvestorInvestmentsQuery { InvestorId = investorId },
            cancellationToken);
        return Ok(result);
    }

    private bool TryGetProfileId(out Guid profileId)
    {
        var raw = User.FindFirst("profileId")?.Value;
        return Guid.TryParse(raw, out profileId);
    }
}
