using CashFlowSA.Models.enums;

namespace CashFlowSA.Models.InvoiceManagement
{
    public class InvoiceDocument
    {
        public Guid InvoiceDocumentId { get; set; }

        public Guid InvoiceId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}

//Purpose:

//The uploaded PDF backing an Invoice. Upload returns immediately (SRS 5.3 AC);
//OCRResult is created asynchronously once RabbitMQ processing picks this up.
