using MediatR;

namespace CashFlowSA.Application.Features.Kyc.GetKycDocumentDownloadUrl
{
    public class GetKycDocumentDownloadUrlQuery : IRequest<KycDocumentDownloadUrlDto>
    {
        public Guid DocumentId { get; set; }
        public Guid UserId { get; set; }
    }

    public class KycDocumentDownloadUrlDto
    {
        public string Url { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
