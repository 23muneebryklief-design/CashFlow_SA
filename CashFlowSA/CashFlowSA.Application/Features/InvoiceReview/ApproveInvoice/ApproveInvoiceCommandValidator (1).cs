using FluentValidation;

namespace CashFlowSA.Application.Features.InvoiceReview.ApproveInvoice
{
    public class ApproveInvoiceCommandValidator : AbstractValidator<ApproveInvoiceCommand>
    {
        public ApproveInvoiceCommandValidator()
        {
            RuleFor(x => x.InvoiceId).NotEmpty();
            RuleFor(x => x.ReviewerId).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(4000);
        }
    }
}
