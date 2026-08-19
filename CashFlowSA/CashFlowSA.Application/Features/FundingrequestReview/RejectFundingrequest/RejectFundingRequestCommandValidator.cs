using FluentValidation;

namespace CashFlowSA.Application.Features.FundingRequestReview.RejectFundingRequest
{
    public class RejectFundingRequestCommandValidator : AbstractValidator<RejectFundingRequestCommand>
    {
        public RejectFundingRequestCommandValidator()
        {
            RuleFor(x => x.FundingRequestId).NotEmpty();
            RuleFor(x => x.ReviewerId).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(1000);
        }
    }
}
