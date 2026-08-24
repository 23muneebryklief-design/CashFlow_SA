using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Analytics.GetFundingVolume;
using CashFlowSA.Application.Features.Analytics.GetRiskDistribution;
using CashFlowSA.Application.Features.Analytics.GetAnalyticsSummary;
using CashFlowSA.Application.Features.Analytics.GetSmeFundingHistory;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    // SRS 5.12: funding volume, ROI, risk distribution -- aggregates data from other modules.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("funding-volume")]
        [Authorize(Roles = "Admin,CreditAnalyst,Auditor,SuperAdmin")]
        public async Task<IActionResult> GetFundingVolume(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var query = new GetFundingVolumeQuery { FromDate = fromDate, ToDate = toDate };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }


        [HttpGet("summary")]
        [Authorize(Roles = "Admin,CreditAnalyst,Auditor,SuperAdmin")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAnalyticsSummaryQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("sme/{smeId}/funding-history")]
        [Authorize(Roles = "Admin,CreditAnalyst,Auditor,SuperAdmin,SME")]
        public async Task<IActionResult> GetSmeFundingHistory(
            Guid smeId,
            CancellationToken cancellationToken)
        {
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                       ?? User.FindFirst("role")?.Value
                       ?? string.Empty;

            if (string.Equals(role, "SME", StringComparison.OrdinalIgnoreCase))
            {
                var profileId = User.FindFirst("profileId")?.Value;
                if (!Guid.TryParse(profileId, out var authenticatedSmeId) || authenticatedSmeId != smeId)
                    return Forbid();
            }

            var result = await _mediator.Send(
                new GetSmeFundingHistoryQuery { SmeId = smeId },
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("risk-distribution")]
        [Authorize(Roles = "Admin,CreditAnalyst,Auditor,SuperAdmin")]
        public async Task<IActionResult> GetRiskDistribution(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetRiskDistributionQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
