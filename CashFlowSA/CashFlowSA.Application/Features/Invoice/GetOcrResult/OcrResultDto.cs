namespace CashFlowSA.Application.Features.Invoice.GetOcrResult
{
    public sealed class OcrResultDto
    {
        public Guid InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? DueDate { get; set; }
        public string? DebtorName { get; set; }
        public decimal ConfidenceScore { get; set; }
        public bool RequiresManualReview { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
