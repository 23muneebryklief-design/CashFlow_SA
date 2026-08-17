using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Invoice.GetInvoice
{
    public class InvoiceDto
    {
        public Guid InvoiceId { get; set; }
        public Guid SMEId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string DebtorName { get; set; } = string.Empty;
        public string DebtorContactDetails { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
        public bool ProcessingComplete { get; set; }
        public string? ReviewNotes { get; set; }
    }
}
