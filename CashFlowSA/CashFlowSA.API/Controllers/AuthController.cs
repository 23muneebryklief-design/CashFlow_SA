using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Auth.LoginUser;
using CashFlowSA.Application.Features.Auth.RegisterInvestor;
using CashFlowSA.Application.Features.Auth.RegisterSme;
using CashFlowSA.Application.Features.Auth.LogoutUser;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        [AllowAnonymous]
        public async Task<IActionResult> RegisterSme(
            [FromBody] RegisterSmeCommand command,
            CancellationToken cancellationToken)
        {
            var smeId = await _mediator.Send(command, cancellationToken);
            return Ok(new { SmeId = smeId });
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(
            [FromBody] LoginUserCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost ("register/investor")]
        [AllowAnonymous]
        public async Task <IActionResult> RegisterInvestor(
            [FromBody] RegisterInvestorCommand command,
            CancellationToken cancellationToken)
        {
            var investorId = await _mediator.Send(command, cancellationToken);
            return Ok(new {InvestorId =investorId});
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var rawUserId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("userId")?.Value;

            if (!Guid.TryParse(rawUserId, out var userId))
                return Unauthorized();

            await _mediator.Send(new LogoutUserCommand { UserId = userId }, cancellationToken);
            return NoContent();
        }

    }
}