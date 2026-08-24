namespace CashFlowSA.Application.Common.Ocr
{
    public interface IInvoiceOcrService
    {
        Task<InvoiceOcrExtraction> ExtractAsync(
            Guid invoiceId,
            string filePath,
            CancellationToken cancellationToken = default);
    }

    public sealed record InvoiceOcrExtraction(
        string? InvoiceNumber,
        decimal? Amount,
        DateTime? DueDate,
        string? DebtorName,
        decimal ConfidenceScore,
        bool RequiresManualReview);
}
