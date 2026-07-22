using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Kyc.DTO
{
    public class KycDocumentDto
    {
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
    }
}