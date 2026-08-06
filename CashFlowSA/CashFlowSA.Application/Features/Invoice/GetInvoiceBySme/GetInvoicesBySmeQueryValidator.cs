using FluentValidation;

namespace CashFlowSA.Application.Features.Invoice.GetInvoicesBySme
{
    public class GetInvoicesBySmeQueryValidator : AbstractValidator<GetInvoicesBySmeQuery>
    {
        public GetInvoicesBySmeQueryValidator()
        {
            RuleFor(x => x.SMEId).NotEmpty().WithMessage("SME ID is required.");
        }
    }
}
