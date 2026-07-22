using FluentValidation;

namespace CashFlowSA.Application.Features.Invoice.GetInvoice
{
    public class GetInvoiceQueryValidator : AbstractValidator<GetInvoiceQuery>
    {
        public GetInvoiceQueryValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEmpty().WithMessage("Invoice ID is required.");
        }
    }
}
