namespace CashFlowSA.Application.Features.Wallet.GetWalletBalance
{
    public class WalletBalanceDto
    {
        public Guid WalletId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
}
