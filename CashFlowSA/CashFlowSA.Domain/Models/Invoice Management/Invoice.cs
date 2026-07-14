using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class Invoice : BaseEntity
    {
        public Guid InvoiceId { get; set; }

        public Guid SMEId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public string DebtorName { get; set; } = string.Empty;

        public string DebtorContactDetails { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime DueDate { get; set; }

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        // True once OCR + risk scoring + AI explanation have all completed for this invoice
        public bool ProcessingComplete { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}

//Purpose:

//Represents an SME's outstanding invoice as it moves through
//Draft -> Submitted -> Under Review -> Approved/Rejected -> Listed (SRS 5.3).
