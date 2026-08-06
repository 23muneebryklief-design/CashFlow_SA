using MediatR;

namespace CashFlowSA.Application.Features.Invoice.GetInvoice
{
    public class GetInvoiceQuery : IRequest<InvoiceDto>
    {
        public Guid InvoiceId { get; set; }
    }
}
