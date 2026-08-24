using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Payments;
using CashFlowSA.Application.Common.Notifications;
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
        private readonly ISandboxPaymentGateway _paymentGateway;
        private readonly INotificationDispatcher _notifications;

        public DepositFundsCommandHandler(
            IApplicationDbContext context,
            ISandboxPaymentGateway paymentGateway,
            INotificationDispatcher notifications)
        {
            _context = context;
            _paymentGateway = paymentGateway;
            _notifications = notifications;
        }

        public async Task<DepositResultDto> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (wallet is null)
                throw new NotFoundException("Wallet not found for this user.");

            var payment = await _paymentGateway.ProcessDepositAsync(
                request.Amount,
                request.CardNumber,
                request.ExpiryMonth,
                request.ExpiryYear,
                request.Cvv,
                cancellationToken);

            if (!payment.Approved)
            {
                return new DepositResultDto
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
                ReferenceId = payment.ProviderTransactionId,
                Description = $"Sandbox deposit via {payment.Provider} using card ending {last4}"
            };

            _context.WalletTransactions.Add(transaction);
            await _context.SaveChangesAsync(cancellationToken);
            await dbTransaction.CommitAsync(cancellationToken);

            await _notifications.DispatchAsync(
                request.UserId,
                NotificationEvent.SystemAnnouncement,
                "Wallet deposit successful",
                $"Your sandbox wallet deposit of R {request.Amount:N2} was approved. New balance: R {wallet.Balance:N2}.",
                new[] { NotificationChannel.InApp },
                cancellationToken);

            return new DepositResultDto
            {
                Success = true,
                Message = "Deposit successful.",
                NewBalance = wallet.Balance,
                TransactionId = transaction.TransactionId,
                ProviderTransactionId = payment.ProviderTransactionId,
                Provider = payment.Provider,
                PaymentStatus = payment.Status
            };
        }
    }
}
