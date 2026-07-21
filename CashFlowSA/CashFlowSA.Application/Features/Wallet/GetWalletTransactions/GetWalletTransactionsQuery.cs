using MediatR;

namespace CashFlowSA.Application.Features.Wallet.GetWalletTransactions
{
    public class GetWalletTransactionsQuery : IRequest<List<WalletTransactionDto>>
    {
        public Guid UserId { get; set; }
    }
}
