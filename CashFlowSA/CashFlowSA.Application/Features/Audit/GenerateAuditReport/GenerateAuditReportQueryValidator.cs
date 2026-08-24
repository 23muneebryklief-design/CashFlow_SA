using FluentValidation;

namespace CashFlowSA.Application.Features.Audit.GenerateAuditReport;

public sealed class GenerateAuditReportQueryValidator : AbstractValidator<GenerateAuditReportQuery>
{
    public GenerateAuditReportQueryValidator()
    {
        RuleFor(x => x.From)
            .LessThanOrEqualTo(x => x.To)
            .When(x => x.From.HasValue && x.To.HasValue)
            .WithMessage("From must be earlier than or equal to To.");

        RuleFor(x => x.EntityType)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.EntityType));

        RuleFor(x => x.EntityId)
            .NotEmpty()
            .When(x => x.EntityId.HasValue);
    }
}
