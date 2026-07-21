using MediatR;

namespace CashFlowSA.Application.Features.Invoice.CorrectInvoiceFields
{
    public class CorrectInvoiceFieldsCommand : IRequest<Unit>
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string DebtorName { get; set; } = string.Empty;
        public string DebtorContactDetails { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
    }
}
