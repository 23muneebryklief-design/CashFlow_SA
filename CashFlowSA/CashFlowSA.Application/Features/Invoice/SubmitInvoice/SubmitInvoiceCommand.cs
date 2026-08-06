using MediatR;

namespace CashFlowSA.Application.Features.Invoice.SubmitInvoice
{
    public class SubmitInvoiceCommand : IRequest<Unit>
    {
        public Guid InvoiceId { get; set; }
    }
}
