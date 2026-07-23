using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Analytics.GetFundingVolume;
using CashFlowSA.Application.Features.Analytics.GetRiskDistribution;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    // SRS 5.12: funding volume, ROI, risk distribution -- aggregates data from other modules.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize( Roles = "Admin ,CreditAnalyst,Auditor")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("funding-volume")]
        public async Task<IActionResult> GetFundingVolume(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            CancellationToken cancellationToken)
        {
            var query = new GetFundingVolumeQuery { FromDate = fromDate, ToDate = toDate };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("risk-distribution")]
        public async Task<IActionResult> GetRiskDistribution(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetRiskDistributionQuery(), cancellationToken);
            return Ok(result);
        }
    }
}
