using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Kyc.SubmitKycApplication;
using CashFlowSA.Application.Features.Kyc;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SME")]
    public class KycController : ControllerBase
    {
        private readonly IMediator _mediator;

        public KycController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitKycApplication(
            [FromBody] SubmitKycApplicationCommand command,
            CancellationToken cancellationToken)
        {
            var applicationId = await _mediator.Send(command, cancellationToken);
            return Ok(new { ApplicationId = applicationId });
        }

    [HttpGet("status/{smeId}")]
        public async Task<IActionResult> GetKycStatus(
        Guid smeId,
        CancellationToken cancellationToken)
        {
            var query = new GetKycStatusQuery { SMEId = smeId };
            var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
        }
    }
}