namespace CashFlowSA.Application.Features.Wallet.WithdrawFunds
{
    public class WithdrawResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
        public Guid? TransactionId { get; set; }
        public Guid? ProviderTransactionId { get; set; }
        public string Provider { get; set; } = "CashFlowSA Sandbox";
        public string PaymentStatus { get; set; } = string.Empty;
    }
}
