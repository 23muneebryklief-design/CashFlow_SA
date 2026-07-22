using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Settlement.GetSettlement;
using CashFlowSA.Application.Features.Settlement.TriggerSettlement;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettlementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public SettlementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{settlementId}")]
        public async Task<IActionResult> GetSettlement(Guid settlementId, CancellationToken cancellationToken)
        {
            var query = new GetSettlementQuery { SettlementId = settlementId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{campaignId}/trigger")]
        public async Task<IActionResult> TriggerSettlement(
            Guid campaignId,
            [FromBody] TriggerSettlementCommand command,
            CancellationToken cancellationToken)
        {
            command.CampaignId = campaignId;
            var settlementId = await _mediator.Send(command, cancellationToken);
            return Ok(new { SettlementId = settlementId });
        }
    }
}
