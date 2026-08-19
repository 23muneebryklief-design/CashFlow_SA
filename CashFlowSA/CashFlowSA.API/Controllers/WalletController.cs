using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Wallet.GetWalletBalance;
using CashFlowSA.Application.Features.Wallet.GetWalletTransactions;
using CashFlowSA.Application.Features.Wallet.DepositFunds;
using CashFlowSA.Application.Features.Wallet.WithdrawFunds;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Investor,SME")]
    public class WalletController : ControllerBase
    {
        private readonly IMediator _mediator;

        public WalletController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId}/balance")]
        public async Task<IActionResult> GetBalance(Guid userId, CancellationToken cancellationToken)
        {
            if (!IsCurrentUser(userId))
                return Forbid();

            var query = new GetWalletBalanceQuery { UserId = userId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{userId}/transactions")]
        public async Task<IActionResult> GetTransactions(Guid userId, CancellationToken cancellationToken)
        {
            if (!IsCurrentUser(userId))
                return Forbid();

            var query = new GetWalletTransactionsQuery { UserId = userId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit(
            [FromBody] DepositFundsCommand command,
            CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
                return Unauthorized();

            // Never trust the client-supplied UserId for a financial operation.
            command.UserId = userId;

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw(
            [FromBody] WithdrawFundsCommand command,
            CancellationToken cancellationToken)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
                return Unauthorized();

            // Never trust the client-supplied UserId for a financial operation.
            command.UserId = userId;

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        private bool IsCurrentUser(Guid userId) =>
            TryGetAuthenticatedUserId(out var authenticatedUserId) && authenticatedUserId == userId;

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var raw = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(raw, out userId);
        }
    }
}
