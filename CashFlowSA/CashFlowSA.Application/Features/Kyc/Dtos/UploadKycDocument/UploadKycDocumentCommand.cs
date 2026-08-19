using MediatR;
using CashFlowSA.Application.Features.Kyc.DTO;

namespace CashFlowSA.Application.Features.Kyc.UploadKycDocument
{
    public class UploadKycDocumentCommand : IRequest<UploadKycDocumentResultDto>
    {
        public Stream FileStream { get; set; } = Stream.Null;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}