using CashFlowSA.Application.Common.Payments;

namespace CashFlowSA.API.Services;

/// <summary>
/// Local payment-provider adapter used for the portfolio/demo sandbox.
/// No card, bank or real-money transaction is ever sent to a financial institution.
/// Test instruments provide deterministic approved/declined outcomes so the
/// complete payment lifecycle can be demonstrated safely.
/// </summary>
public sealed class SandboxPaymentGateway : ISandboxPaymentGateway
{
    public Task<SandboxPaymentResult> ProcessDepositAsync(
        decimal amount,
        string cardNumber,
        string expiryMonth,
        string expiryYear,
        string cvv,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var transactionId = Guid.NewGuid();
        var last4 = GetLast4(cardNumber);

        if (cardNumber.EndsWith("0002", StringComparison.Ordinal))
        {
            return Task.FromResult(new SandboxPaymentResult
            {
                Approved = false,
                ProviderTransactionId = transactionId,
                Status = "Declined",
                Message = "Sandbox payment declined. Test decline card ending in 0002 was used.",
                FailureCode = "TEST_DECLINED"
            });
        }

        return Task.FromResult(new SandboxPaymentResult
        {
            Approved = true,
            ProviderTransactionId = transactionId,
            Status = "Approved",
            Message = $"Sandbox payment approved for card ending {last4}."
        });
    }

    public Task<SandboxPaymentResult> ProcessWithdrawalAsync(
        decimal amount,
        string accountNumber,
        string bankName,
        string branchCode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var transactionId = Guid.NewGuid();
        var last4 = GetLast4(accountNumber);

        if (accountNumber.EndsWith("0002", StringComparison.Ordinal))
        {
            return Task.FromResult(new SandboxPaymentResult
            {
                Approved = false,
                ProviderTransactionId = transactionId,
                Status = "Declined",
                Message = "Sandbox payout declined. Test decline account ending in 0002 was used.",
                FailureCode = "TEST_DECLINED"
            });
        }

        return Task.FromResult(new SandboxPaymentResult
        {
            Approved = true,
            ProviderTransactionId = transactionId,
            Status = "Approved",
            Message = $"Sandbox payout approved for account ending {last4}."
        });
    }

    private static string GetLast4(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "----";

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length <= 4 ? digits : digits[^4..];
    }
}
