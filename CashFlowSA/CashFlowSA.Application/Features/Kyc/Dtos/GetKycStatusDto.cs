using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Kyc.DTO
{
    public class KycDocumentStatusDto
    {
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class KycStatusDto
    {
        public Guid ApplicationId { get; set; }
        public KycStatus Status { get; set; }
        public DateTime ApplicationDate { get; set; }
        public List<KycDocumentStatusDto> Documents { get; set; } = new();
    }
}
