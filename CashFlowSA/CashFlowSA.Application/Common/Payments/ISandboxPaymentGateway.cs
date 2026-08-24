namespace CashFlowSA.Application.Common.Payments;

public interface ISandboxPaymentGateway
{
    Task<SandboxPaymentResult> ProcessDepositAsync(
        decimal amount,
        string cardNumber,
        string expiryMonth,
        string expiryYear,
        string cvv,
        CancellationToken cancellationToken = default);

    Task<SandboxPaymentResult> ProcessWithdrawalAsync(
        decimal amount,
        string accountNumber,
        string bankName,
        string branchCode,
        CancellationToken cancellationToken = default);
}
