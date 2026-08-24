using CashFlowSA.Domain.Models.Enums;
using FluentValidation;

namespace CashFlowSA.Application.Features.Risk.OverrideRiskScore;

public sealed class OverrideRiskScoreCommandValidator : AbstractValidator<OverrideRiskScoreCommand>
{
    public OverrideRiskScoreCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.RiskScore).InclusiveBetween(0m, 100m);
        RuleFor(x => x.RiskGrade).IsInEnum();
        RuleFor(x => x.Justification)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
