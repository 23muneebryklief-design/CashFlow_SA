using FluentValidation;

namespace CashFlowSA.Application.Features.Marketplace.GetListings
{
    public class GetListingsQueryValidator : AbstractValidator<GetListingsQuery>
    {
        public GetListingsQueryValidator()
        {
            // RiskGrade and Industry are enums -- IsInEnum() guards against an
            // out-of-range value being sent (e.g. someone passing riskGrade=99).
            RuleFor(x => x.RiskGrade)
                .IsInEnum()
                .When(x => x.RiskGrade.HasValue);

            RuleFor(x => x.Industry)
                .IsInEnum()
                .When(x => x.Industry.HasValue);

            RuleFor(x => x.MinAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinAmount.HasValue)
                .WithMessage("Minimum amount cannot be negative.");

            RuleFor(x => x.MaxAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxAmount.HasValue)
                .WithMessage("Maximum amount cannot be negative.");

            // Cross-field rule: only meaningful when both are supplied.
            RuleFor(x => x)
                .Must(x => !x.MinAmount.HasValue || !x.MaxAmount.HasValue || x.MinAmount <= x.MaxAmount)
                .WithMessage("Minimum amount must not be greater than maximum amount.")
                .WithName("AmountRange");
        }
    }
}