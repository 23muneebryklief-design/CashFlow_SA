namespace CashFlowSA.Application.Common.Payments;

public sealed class SandboxPaymentResult
{
    public bool Approved { get; init; }
    public Guid ProviderTransactionId { get; init; }
    public string Provider { get; init; } = "CashFlowSA Sandbox";
    public string Status { get; init; } = "Approved";
    public string Message { get; init; } = string.Empty;
    public string? FailureCode { get; init; }
}
