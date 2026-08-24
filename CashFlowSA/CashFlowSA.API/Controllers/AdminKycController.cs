using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.AdminKyc.ApproveKycApplication;
using CashFlowSA.Application.Features.AdminKyc.RejectKycApplication;
using CashFlowSA.Application.Features.AdminKyc.GetPendingKycApplications;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    // Ops-portal side: Credit Analyst / Admin reviews KYC submissions.
    [ApiController]
    [Route("api/admin/kyc")]
    [Authorize(Roles = "Admin,CreditAnalyst,SuperAdmin")]
    public class AdminKycController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminKycController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("{applicationId}/approve")]
        public async Task<IActionResult> ApproveKycApplication(
            Guid applicationId,
            [FromBody] ApproveKycApplicationCommand command,
            CancellationToken cancellationToken)
        {
            command.ApplicationId = applicationId;

            if (!TryGetAuthenticatedUserId(out var reviewerId))
                return Unauthorized();

            // Reviewer identity must come from the authenticated token, never
            // from a client-supplied ReviewerId.
            command.ReviewerId = reviewerId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpPost("{applicationId}/reject")]
        public async Task<IActionResult> RejectKycApplication(
            Guid applicationId,
            [FromBody] RejectKycApplicationCommand command,
            CancellationToken cancellationToken)
        {
            command.ApplicationId = applicationId;

            if (!TryGetAuthenticatedUserId(out var reviewerId))
                return Unauthorized();

            // Reviewer identity must come from the authenticated token, never
            // from a client-supplied ReviewerId.
            command.ReviewerId = reviewerId;
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingApplications(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPendingKycApplicationsQuery(), cancellationToken);
            return Ok(result);
        }

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var rawUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("userId")?.Value;

            return Guid.TryParse(rawUserId, out userId);
        }
    }
}
