using FluentValidation;

namespace CashFlowSA.Application.Features.Audit.GetAuditLogs
{
    public class GetAuditLogsQueryValidator : AbstractValidator<GetAuditLogsQuery>
    {
        public GetAuditLogsQueryValidator()
        {
            RuleFor(x => x.Action)
                .IsInEnum()
                .When(x => x.Action.HasValue);

            RuleFor(x => x.EntityType)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.EntityType));
        }
    }
}
