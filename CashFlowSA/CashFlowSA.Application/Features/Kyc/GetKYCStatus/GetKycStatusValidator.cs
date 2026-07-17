using FluentValidation;

namespace CashFlowSA.Application.Features.Kyc.GetKycStatus
{
    public class GetKycStatusQueryValidator : AbstractValidator<GetKycStatusQuery>
    {
        public GetKycStatusQueryValidator()
        {
            RuleFor(x => x.SMEId)
                .NotEmpty().WithMessage("SME ID is required.");
        }
    }
}