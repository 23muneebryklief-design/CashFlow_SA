using FluentValidation;

namespace CashFlowSA.Application.Features.Wallet.GetWalletTransactions
{
    public class GetWalletTransactionsQueryValidator : AbstractValidator<GetWalletTransactionsQuery>
    {
        public GetWalletTransactionsQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        }
    }
}
