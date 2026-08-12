using FluentValidation;

namespace CashFlowSA.Application.Features.AuditorKyc.RejectKycDocument
{
    public class RejectKycDocumentCommandValidator : AbstractValidator<RejectKycDocumentCommand>
    {
        public RejectKycDocumentCommandValidator()
        {
            RuleFor(x => x.DocumentId).NotEmpty();
            RuleFor(x => x.ReviewerId).NotEmpty();

            // Unlike approval, a rejection needs a reason on file -- the SME
            // has to know what to fix and resubmit.
            RuleFor(x => x.Notes)
                .NotEmpty().WithMessage("A reason is required when rejecting a document.")
                .MaximumLength(4000);
        }
    }
}
