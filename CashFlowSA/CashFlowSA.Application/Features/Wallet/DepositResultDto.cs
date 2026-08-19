namespace CashFlowSA.Application.Features.Wallet.DepositFunds
{
    public class DepositResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal NewBalance { get; set; }
        public Guid? TransactionId { get; set; }
    }
}
