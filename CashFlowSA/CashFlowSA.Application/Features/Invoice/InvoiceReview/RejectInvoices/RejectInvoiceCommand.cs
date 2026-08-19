using MediatR;

namespace CashFlowSA.Application.Features.InvoiceReview.RejectInvoice
{
    public class RejectInvoiceCommand : IRequest<Unit>
    {
        public Guid InvoiceId { get; set; }
        public Guid ReviewerId { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
