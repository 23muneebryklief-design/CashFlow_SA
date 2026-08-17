using FluentValidation;

namespace CashFlowSA.Application.Features.AuditorKyc.GetKycDocumentDownloadUrl
{
    public class GetKycDocumentDownloadUrlQueryValidator : AbstractValidator<GetKycDocumentDownloadUrlQuery>
    {
        public GetKycDocumentDownloadUrlQueryValidator()
        {
            RuleFor(x => x.DocumentId).NotEmpty();
        }
    }
}
