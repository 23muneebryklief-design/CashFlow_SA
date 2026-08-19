using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CashFlowSA.Application.Features.Wallet.DepositFunds
{
    public class DepositFundsCommandHandler : IRequestHandler<DepositFundsCommand, DepositResultDto>
    {
        private readonly IApplicationDbContext _context;

        public DepositFundsCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DepositResultDto> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet is null)
                throw new NotFoundException("Wallet not found for this user.");

            // Sandbox gateway simulation (SRS 5.6 -- no real payment rail).
            // Test-card convention, same idea as real sandboxes (e.g. Stripe/Peach
            // test cards): a card number ending in 0002 always simulates a
            // decline, so the flow can be tested end-to-end without a real bank.
            if (request.CardNumber.EndsWith("0002"))
            {
                return new DepositResultDto
                {
                    Success = false,
                    Message = "Card declined by sandbox gateway (test decline card).",
                    NewBalance = wallet.Balance,
                    TransactionId = null
                };
            }

            await using var dbTransaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            // Re-read the wallet inside the serializable transaction so concurrent
            // deposits/withdrawals cannot overwrite each other.
            wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet is null)
                throw new NotFoundException("Wallet not found for this user.");

            wallet.Balance += request.Amount;

            var last4 = request.CardNumber.Length >= 4
                ? request.CardNumber[^4..]
                : request.CardNumber;

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                Type = WalletTransactionType.Credit,
                Amount = request.Amount,
                ReferenceType = "SandboxDeposit",
                ReferenceId = null,
                Description = $"Sandbox deposit via card ending {last4}"
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            return new DepositResultDto
            {
                Success = true,
                Message = "Deposit successful.",
                NewBalance = wallet.Balance,
                TransactionId = transaction.TransactionId
            };
        }
    }
}
