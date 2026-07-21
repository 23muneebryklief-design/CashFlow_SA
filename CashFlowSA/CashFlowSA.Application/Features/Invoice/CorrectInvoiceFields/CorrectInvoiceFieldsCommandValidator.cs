using FluentValidation;

namespace CashFlowSA.Application.Features.Invoice.CorrectInvoiceFields
{
    public class CorrectInvoiceFieldsCommandValidator : AbstractValidator<CorrectInvoiceFieldsCommand>
    {
        public CorrectInvoiceFieldsCommandValidator()
        {
            RuleFor(x => x.InvoiceId).NotEmpty();
            RuleFor(x => x.InvoiceNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.DebtorName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.DebtorContactDetails).NotEmpty().MaximumLength(300);
            RuleFor(x => x.Amount).GreaterThan(0);
            RuleFor(x => x.IssueDate).LessThanOrEqualTo(x => x.DueDate)
                .WithMessage("Issue date must be on or before the due date.");
        }
    }
}
