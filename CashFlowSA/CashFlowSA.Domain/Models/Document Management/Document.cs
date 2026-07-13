using CashFlowSA.Models.enums;

namespace CashFlowSA.Models.DocumentManagement
{
    public class Document
    {
        public Guid DocumentId { get; set; }

        public Guid UploadedByUserId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileType { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}