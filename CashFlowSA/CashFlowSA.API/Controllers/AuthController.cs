using CashFlowSA.Application.Features.Auth.Commands.RegisterSme;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register/sme")]
        public async Task<IActionResult> RegisterSme(
            [FromBody] RegisterSmeCommand command,
            CancellationToken cancellationToken)
        {
            var smeId = await _mediator.Send(command, cancellationToken);
            return Ok(new { SmeId = smeId });
        }
    }
}