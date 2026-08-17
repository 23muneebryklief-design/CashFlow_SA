using FluentValidation;

namespace CashFlowSA.Application.Features.AuditorKyc.GetKycDocumentsForReview
{
    public class GetKycDocumentsForReviewQueryValidator : AbstractValidator<GetKycDocumentsForReviewQuery>
    {
        public GetKycDocumentsForReviewQueryValidator()
        {
            RuleFor(x => x.StatusFilter)
                .IsInEnum()
                .When(x => x.StatusFilter.HasValue);
        }
    }
}
