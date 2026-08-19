using FluentValidation;

namespace CashFlowSA.Application.Features.InvoiceReview.GetInvoicesForReview
{
    public class GetInvoicesForReviewQueryValidator : AbstractValidator<GetInvoicesForReviewQuery>
    {
        public GetInvoicesForReviewQueryValidator()
        {
            RuleFor(x => x.StatusFilter)
                .IsInEnum()
                .When(x => x.StatusFilter.HasValue);
        }
    }
}
