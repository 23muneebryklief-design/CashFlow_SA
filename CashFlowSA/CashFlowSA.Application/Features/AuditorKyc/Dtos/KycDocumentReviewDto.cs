using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.AuditorKyc.Dtos
{
    public class KycDocumentReviewDto
    {
        public Guid DocumentId { get; set; }
        public DocumentType DocumentType { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public DocumentStatus Status { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
