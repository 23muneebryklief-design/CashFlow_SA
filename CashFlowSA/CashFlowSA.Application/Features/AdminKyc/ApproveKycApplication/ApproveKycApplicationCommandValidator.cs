using FluentValidation;

namespace CashFlowSA.Application.Features.AdminKyc.ApproveKycApplication
{
    public class ApproveKycApplicationCommandValidator : AbstractValidator<ApproveKycApplicationCommand>
    {
        public ApproveKycApplicationCommandValidator()
        {
            RuleFor(x => x.ApplicationId).NotEmpty();
            RuleFor(x => x.ReviewerId).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}
