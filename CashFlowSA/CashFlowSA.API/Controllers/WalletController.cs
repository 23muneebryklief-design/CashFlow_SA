using MediatR;
using Microsoft.AspNetCore.Mvc;
using CashFlowSA.Application.Features.Wallet.GetWalletBalance;
using CashFlowSA.Application.Features.Wallet.GetWalletTransactions;
using CashFlowSA.Application.Features.Wallet.DepositFunds;
using Microsoft.AspNetCore.Authorization;

namespace CashFlowSA.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles ="Investor,SME")]
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
            Console.WriteLine($"Reached WalletController: {userId}");

            var query = new GetWalletBalanceQuery
            {
                UserId = userId
            };

            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }
        [HttpGet("{userId}/transactions")]
        public async Task<IActionResult> GetTransactions(Guid userId, CancellationToken cancellationToken)
        {
            var query = new GetWalletTransactionsQuery { UserId = userId };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositFundsCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
