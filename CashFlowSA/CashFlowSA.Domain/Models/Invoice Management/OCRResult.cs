namespace CashFlowSA.Domain.Models
{
    public class OCRResult : BaseEntity
    {
        public Guid OCRResultId { get; set; }

        public Guid InvoiceId { get; set; }

        public string? ExtractedInvoiceNumber { get; set; }

        public decimal? ExtractedAmount { get; set; }

        public DateTime? ExtractedDueDate { get; set; }

        public string? ExtractedDebtorName { get; set; }

        // 0-100 confidence for the overall extraction
        public decimal ConfidenceScore { get; set; }

        // SRS 5.10 AC: low-confidence fields must be flagged for manual review, not silently accepted
        public bool RequiresManualReview { get; set; }

        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//Stores what OCR extracted from the InvoiceDocument so the SME can review/correct
//fields before the Invoice moves to Submitted (SRS 5.3 AC: OCR failures fall back
//to manual entry rather than blocking the upload).
