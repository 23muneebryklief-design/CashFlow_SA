using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Wallet.WithdrawFunds
{
    public class WithdrawFundsCommandHandler : IRequestHandler<WithdrawFundsCommand, WithdrawResultDto>
    {
        private readonly IApplicationDbContext _context;

        public WithdrawFundsCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WithdrawResultDto> Handle(WithdrawFundsCommand request, CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet is null)
                throw new NotFoundException("Wallet not found for this user.");

            // Authoritative server-side check -- the frontend also blocks this,
            // but that's UX only and must never be trusted. Soft-fail (200 OK,
            // Success=false) rather than throw, same pattern as a declined
            // deposit card, so the modal shows a normal inline error instead
            // of an unhandled exception.
            if (request.Amount > wallet.Balance)
            {
                return new WithdrawResultDto
                {
                    Success = false,
                    Message = "Insufficient wallet balance for this withdrawal.",
                    NewBalance = wallet.Balance,
                    TransactionId = null
                };
            }

            // Sandbox payout simulation (SRS 5.6 -- no real payment rail).
            // Same test-account convention as DepositFunds' test-card decline:
            // an account number ending in 0002 always simulates a rejection,
            // so the flow can be tested end-to-end without a real bank.
            if (request.AccountNumber.EndsWith("0002"))
            {
                return new WithdrawResultDto
                {
                    Success = false,
                    Message = "Withdrawal rejected by sandbox payout gateway (test decline account).",
                    NewBalance = wallet.Balance,
                    TransactionId = null
                };
            }

            wallet.Balance -= request.Amount;

            var last4 = request.AccountNumber.Length >= 4
                ? request.AccountNumber[^4..]
                : request.AccountNumber;

            var transaction = new WalletTransaction
            {
                TransactionId = Guid.NewGuid(),
                WalletId = wallet.WalletId,
                Type = WalletTransactionType.Debit,
                Amount = request.Amount,
                ReferenceType = "SandboxWithdrawal",
                ReferenceId = null,
                Description = $"Sandbox withdrawal to {request.BankName} account ending {last4}"
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);

            return new WithdrawResultDto
            {
                Success = true,
                Message = "Withdrawal successful.",
                NewBalance = wallet.Balance,
                TransactionId = transaction.TransactionId
            };
        }
    }
}