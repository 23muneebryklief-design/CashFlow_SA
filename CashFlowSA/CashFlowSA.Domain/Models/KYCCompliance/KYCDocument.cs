using CashFlowSA.Domain.Models.Enums;
namespace CashFlowSA.Domain.Models
{
    public class KYCDocuments
    {
        public Guid DocumentId { get; set; }
        public Guid UserId { get; set; }

        // Set at submission time (see SubmitKycApplicationCommandHandler) --
        // this is the real link between a document and the application it
        // belongs to, replacing the old "UploadedAt >= ApplicationDate"
        // heuristic that used to stand in for it.
        public Guid? KYCApplicationId { get; set; }

        public DocumentType DocumentType { get; set; } = DocumentType.Other;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public long FileSize { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        // Populated once an Auditor (or Admin) reviews this specific document --
        // distinct from KYCReview, which records the decision on the whole
        // application rather than an individual document.
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNotes { get; set; }
    }
}

//Purpose:

//Compliance verification.
