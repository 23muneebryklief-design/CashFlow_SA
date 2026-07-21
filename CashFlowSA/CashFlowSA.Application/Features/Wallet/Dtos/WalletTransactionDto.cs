using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Wallet.GetWalletTransactions
{
    public class WalletTransactionDto
    {
        public Guid TransactionId { get; set; }
        public WalletTransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceType { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
