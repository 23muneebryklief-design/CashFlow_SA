using FluentValidation;

namespace CashFlowSA.Application.Features.InvoiceReview.RejectInvoice
{
    public class RejectInvoiceCommandValidator : AbstractValidator<RejectInvoiceCommand>
    {
        public RejectInvoiceCommandValidator()
        {
            RuleFor(x => x.InvoiceId).NotEmpty();
            RuleFor(x => x.ReviewerId).NotEmpty();

            RuleFor(x => x.Notes)
                .NotEmpty().WithMessage("A reason is required when rejecting an invoice.")
                .MaximumLength(4000);
        }
    }
}
