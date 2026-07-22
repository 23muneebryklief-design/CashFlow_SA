using FluentValidation;
using CashFlowSA.Application.Features.Kyc.SubmitKycApplication;

namespace CashFlowSA.Application.Features.Kyc.SubmitKycApplication 
{
    public class SubmitKycApplicationCommandValidator : AbstractValidator<SubmitKycApplicationCommand>
    {
        public SubmitKycApplicationCommandValidator()
        {
            RuleFor(x => x.SMEId)
                .NotEmpty().WithMessage("SME ID is required.");

            RuleFor(x => x.Documents)
                .NotEmpty().WithMessage("At least one document is required.");

            RuleForEach(x => x.Documents).ChildRules(doc =>
            {
                doc.RuleFor(d => d.DocumentType).IsInEnum();
                doc.RuleFor(d => d.FileName).NotEmpty().MaximumLength(255);
                doc.RuleFor(d => d.FilePath).NotEmpty();
                doc.RuleFor(d => d.FileSize).GreaterThan(0);
            });
        }
    }
}