using MediatR;

namespace CashFlowSA.Application.Features.InvoiceReview.ApproveInvoice
{
    public class ApproveInvoiceCommand : IRequest<Unit>
    {
        public Guid InvoiceId { get; set; }
        public Guid ReviewerId { get; set; }
        public string? Notes { get; set; }
    }
}
