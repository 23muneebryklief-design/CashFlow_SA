using FluentValidation;

namespace CashFlowSA.Application.Features.AuditorKyc.ApproveKycDocument
{
    public class ApproveKycDocumentCommandValidator : AbstractValidator<ApproveKycDocumentCommand>
    {
        public ApproveKycDocumentCommandValidator()
        {
            RuleFor(x => x.DocumentId).NotEmpty();
            RuleFor(x => x.ReviewerId).NotEmpty();
            RuleFor(x => x.Notes).MaximumLength(4000);
        }
    }
}
