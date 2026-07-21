using FluentValidation;

namespace CashFlowSA.Application.Features.Wallet.GetWalletBalance
{
    public class GetWalletBalanceQueryValidator : AbstractValidator<GetWalletBalanceQuery>
    {
        public GetWalletBalanceQueryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
        }
    }
}
