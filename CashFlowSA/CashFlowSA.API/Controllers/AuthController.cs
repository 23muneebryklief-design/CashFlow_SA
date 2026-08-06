using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Auth.LoginUser;
using CashFlowSA.Application.Features.Auth.RegisterInvestor;
using CashFlowSA.Application.Features.Auth.RegisterSme;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
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
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost ("register/investor")]
        public async Task <IActionResult> RegisterInvestor(
            [FromBody] RegisterInvestorCommand command,
            CancellationToken cancellationToken)
        {
            var investorId = await _mediator.Send(command, cancellationToken);
            return Ok(new {InvestorId =investorId});
        }
    }
}