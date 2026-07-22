using MediatR;

namespace CashFlowSA.Application.Features.Wallet.GetWalletBalance
{
    public class GetWalletBalanceQuery : IRequest<WalletBalanceDto>
    {
        public Guid UserId { get; set; }
    }
}
