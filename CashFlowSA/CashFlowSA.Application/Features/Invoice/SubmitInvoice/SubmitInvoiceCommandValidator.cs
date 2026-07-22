using FluentValidation;

namespace CashFlowSA.Application.Features.Invoice.SubmitInvoice
{
    public class SubmitInvoiceCommandValidator : AbstractValidator<SubmitInvoiceCommand>
    {
        public SubmitInvoiceCommandValidator()
        {
            RuleFor(x => x.InvoiceId).NotEmpty().WithMessage("Invoice ID is required.");
        }
    }
}
