using FluentValidation;

namespace CashFlowSA.Application.Features.Wallet.WithdrawFunds
{
    public class WithdrawFundsCommandValidator : AbstractValidator<WithdrawFundsCommand>
    {
        public WithdrawFundsCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Withdrawal amount must be greater than 0.")
                .LessThanOrEqualTo(1_000_000).WithMessage("Withdrawal amount cannot exceed R1,000,000.");

            RuleFor(x => x.AccountHolderName)
                .NotEmpty().WithMessage("Account holder name is required.")
                .MaximumLength(100).WithMessage("Account holder name is too long.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Bank name is required.");

            // South African bank account numbers are typically 6-11 digits
            // depending on the bank.
            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Account number is required.")
                .Matches(@"^\d{6,11}$").WithMessage("Account number must be 6-11 digits.");

            // SA universal branch codes are 6 digits.
            RuleFor(x => x.BranchCode)
                .NotEmpty().WithMessage("Branch code is required.")
                .Matches(@"^\d{6}$").WithMessage("Branch code must be 6 digits.");
        }
    }
}