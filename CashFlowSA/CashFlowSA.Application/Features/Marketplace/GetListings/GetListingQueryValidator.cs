using FluentValidation;

namespace CashFlowSA.Application.Features.Marketplace.GetListings
{
    public class GetListingsQueryValidator : AbstractValidator<GetListingsQuery>
    {
        public GetListingsQueryValidator()
        {
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

            RuleFor(x => x.MinTenorDays)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinTenorDays.HasValue)
                .WithMessage("Minimum tenor cannot be negative.");

            RuleFor(x => x.MaxTenorDays)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxTenorDays.HasValue)
                .WithMessage("Maximum tenor cannot be negative.");

            RuleFor(x => x)
                .Must(x => !x.MinAmount.HasValue || !x.MaxAmount.HasValue || x.MinAmount <= x.MaxAmount)
                .WithMessage("Minimum amount must not be greater than maximum amount.")
                .WithName("AmountRange");

            RuleFor(x => x)
                .Must(x => !x.MinTenorDays.HasValue || !x.MaxTenorDays.HasValue || x.MinTenorDays <= x.MaxTenorDays)
                .WithMessage("Minimum tenor must not be greater than maximum tenor.")
                .WithName("TenorRange");
        }
    }
}
