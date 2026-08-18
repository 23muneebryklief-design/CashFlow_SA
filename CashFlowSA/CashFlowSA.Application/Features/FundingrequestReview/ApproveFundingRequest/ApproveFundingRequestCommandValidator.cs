using FluentValidation;

namespace CashFlowSA.Application.Features.FundingRequestReview.ApproveFundingRequest
{
    public class ApproveFundingRequestCommandValidator : AbstractValidator<ApproveFundingRequestCommand>
    {
        public ApproveFundingRequestCommandValidator()
        {
            RuleFor(x => x.FundingRequestId).NotEmpty();
            RuleFor(x => x.ReviewerId).NotEmpty();

            RuleFor(x => x.ExpectedReturnRate)
                .GreaterThan(0).When(x => x.ExpectedReturnRate.HasValue)
                .WithMessage("Expected return rate must be greater than zero.");

            RuleFor(x => x.FundingDeadline)
                .GreaterThan(DateTime.UtcNow).When(x => x.FundingDeadline.HasValue)
                .WithMessage("Funding deadline must be in the future.");
        }
    }
}
