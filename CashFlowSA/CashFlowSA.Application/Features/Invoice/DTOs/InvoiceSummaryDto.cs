using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Invoice.GetInvoicesBySme
{
    public class InvoiceSummaryDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public InvoiceStatus Status { get; set; }
    }
}
