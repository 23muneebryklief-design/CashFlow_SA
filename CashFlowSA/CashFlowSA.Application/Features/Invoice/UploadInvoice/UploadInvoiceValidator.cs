using FluentValidation;
using CashFlowSA.Application.Features.Invoice.UploadInvoice;
using CashFlowSA.Domain.Models;

namespace CashFlowSA.Application.Features.Invoice.UploadInvoice
{
    public class UploadInvoiceCommandValidator : AbstractValidator<UploadInvoiceCommand>
    {
        public UploadInvoiceCommandValidator()
        {
            
            RuleFor(x => x.SMEId)
                .NotEmpty().WithMessage("SME ID is required");
            
            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255);

            RuleFor(x => x.FilePath)
                .NotEmpty().WithMessage("File path is required.");

            RuleFor(x => x.FileSize)
                .GreaterThan(0).WithMessage("File size must be greater than zero.");
        }
    }
}