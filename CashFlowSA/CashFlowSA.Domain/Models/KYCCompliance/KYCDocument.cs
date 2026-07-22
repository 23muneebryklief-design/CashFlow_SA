using CashFlowSA.Domain.Models.Enums;
namespace CashFlowSA.Domain.Models
{
    public class KYCDocuments
    {
        public Guid DocumentId { get; set; }
        public Guid UserId { get; set; }
        public DocumentType DocumentType { get; set; } = DocumentType.Other;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public long FileSize { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    }
}

//Purpose:

//Compliance verification.
