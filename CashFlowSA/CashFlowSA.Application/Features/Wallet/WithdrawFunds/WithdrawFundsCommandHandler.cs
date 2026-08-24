using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Payments;
using CashFlowSA.Application.Common.Notifications;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CashFlowSA.Application.Features.Wallet.WithdrawFunds
{
    public class WithdrawFundsCommandHandler : IRequestHandler<WithdrawFundsCommand, WithdrawResultDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly ISandboxPaymentGateway _paymentGateway;
        private readonly INotificationDispatcher _notifications;

        public WithdrawFundsCommandHandler(
            IApplicationDbContext context,
            ISandboxPaymentGateway paymentGateway,
            INotificationDispatcher notifications)
        {
            _context = context;
            _paymentGateway = paymentGateway;
            _notifications = notifications;
        }

        public async Task<WithdrawResultDto> Handle(WithdrawFundsCommand request, CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet is null)
                throw new NotFoundException("Wallet not found for this user.");

            await using var dbTransaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            // Re-read inside the serializable transaction. This prevents two
            // simultaneous withdrawals from both observing the same balance.
            wallet = await _context.Wallets
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

            var payment = await _paymentGateway.ProcessWithdrawalAsync(
                request.Amount,
                request.AccountNumber,
                request.BankName,
                request.BranchCode,
                cancellationToken);

            if (!payment.Approved)
            {
                return new WithdrawResultDto
                {
                    Success = false,
                    Message = payment.Message,
                    NewBalance = wallet.Balance,
                    TransactionId = null,
                    ProviderTransactionId = payment.ProviderTransactionId,
                    Provider = payment.Provider,
                    PaymentStatus = payment.Status
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
                ReferenceId = payment.ProviderTransactionId,
                Description = $"Sandbox withdrawal via {payment.Provider} to {request.BankName} account ending {last4}"
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            await _notifications.DispatchAsync(
                request.UserId,
                NotificationEvent.SystemAnnouncement,
                "Wallet withdrawal successful",
                $"Your sandbox wallet withdrawal of R {request.Amount:N2} was approved. New balance: R {wallet.Balance:N2}.",
                new[] { NotificationChannel.InApp },
                cancellationToken);

            return new WithdrawResultDto
            {
                Success = true,
                Message = "Withdrawal successful.",
                NewBalance = wallet.Balance,
                TransactionId = transaction.TransactionId,
                ProviderTransactionId = payment.ProviderTransactionId,
                Provider = payment.Provider,
                PaymentStatus = payment.Status
            };
        }
    }
}