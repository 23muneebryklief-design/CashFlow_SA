using MediatR;

namespace CashFlowSA.Application.Features.Invoice.GetInvoicesBySme
{
    public class GetInvoicesBySmeQuery : IRequest<List<InvoiceSummaryDto>>
    {
        public Guid SMEId { get; set; }
    }
}
