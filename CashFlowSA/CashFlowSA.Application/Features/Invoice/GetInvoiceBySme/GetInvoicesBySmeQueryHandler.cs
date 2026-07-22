using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Invoice.GetInvoicesBySme
{
    public class GetInvoicesBySmeQueryHandler : IRequestHandler<GetInvoicesBySmeQuery, List<InvoiceSummaryDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetInvoicesBySmeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InvoiceSummaryDto>> Handle(GetInvoicesBySmeQuery request, CancellationToken cancellationToken)
        {
            // No NotFoundException here: an SME with zero invoices is a valid
            // state (e.g. just registered), not an error -- return an empty list.
            return await _context.Invoices
                .Where(i => i.SMEId == request.SMEId)
                .OrderByDescending(i => i.IssueDate)
                .Select(i => new InvoiceSummaryDto
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    Amount = i.Amount,
                    DueDate = i.DueDate,
                    Status = i.Status
                })
                .ToListAsync(cancellationToken);
        }
    }
}
