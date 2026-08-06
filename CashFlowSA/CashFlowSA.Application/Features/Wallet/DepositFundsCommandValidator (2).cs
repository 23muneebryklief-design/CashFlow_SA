using FluentValidation;

namespace CashFlowSA.Application.Features.Wallet.DepositFunds
{
    public class DepositFundsCommandValidator : AbstractValidator<DepositFundsCommand>
    {
        public DepositFundsCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Deposit amount must be greater than 0.")
                .LessThanOrEqualTo(1_000_000).WithMessage("Deposit amount cannot exceed R1,000,000.");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("Card number is required.")
                .Matches(@"^\d{13,19}$").WithMessage("Card number must be 13-19 digits.");

            RuleFor(x => x.ExpiryMonth)
                .NotEmpty().WithMessage("Expiry month is required.")
                .Matches(@"^(0[1-9]|1[0-2])$").WithMessage("Expiry month must be between 01 and 12.");

            RuleFor(x => x.ExpiryYear)
                .NotEmpty().WithMessage("Expiry year is required.")
                .Matches(@"^\d{2}$|^\d{4}$").WithMessage("Expiry year must be YY or YYYY.");

            RuleFor(x => x.Cvv)
                .NotEmpty().WithMessage("CVV is required.")
                .Matches(@"^\d{3,4}$").WithMessage("CVV must be 3-4 digits.");

            RuleFor(x => x)
                .Must(NotBeExpired)
                .WithMessage("Card has expired.")
                .WithName("ExpiryYear")
                .When(x =>
                    System.Text.RegularExpressions.Regex.IsMatch(x.ExpiryMonth, @"^(0[1-9]|1[0-2])$") &&
                    System.Text.RegularExpressions.Regex.IsMatch(x.ExpiryYear, @"^\d{2}$|^\d{4}$"));
        }

        private static bool NotBeExpired(DepositFundsCommand command)
        {
            var year = command.ExpiryYear.Length == 2
                ? 2000 + int.Parse(command.ExpiryYear)
                : int.Parse(command.ExpiryYear);
            var month = int.Parse(command.ExpiryMonth);

            var expiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
            return expiry >= DateTime.UtcNow.Date;
    }
}
