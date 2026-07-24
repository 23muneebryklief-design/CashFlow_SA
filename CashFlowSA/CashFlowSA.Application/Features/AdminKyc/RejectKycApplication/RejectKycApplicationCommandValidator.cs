using FluentValidation;

namespace CashFlowSA.Application.Features.AdminKyc.RejectKycApplication
{
    public class RejectKycApplicationCommandValidator : AbstractValidator<RejectKycApplicationCommand>
    {
        public RejectKycApplicationCommandValidator()
        {
            RuleFor(x => x.ApplicationId).NotEmpty();
            RuleFor(x => x.ReviewerId).NotEmpty();

            // Required, not optional, unlike Approve's Notes -- SRS 5.2 AC:
            // a rejection must let the SME know what to correct before resubmitting.
            RuleFor(x => x.Notes)
                .NotEmpty().WithMessage("A rejection reason is required.")
                .MaximumLength(1000);
        }
    }
}
