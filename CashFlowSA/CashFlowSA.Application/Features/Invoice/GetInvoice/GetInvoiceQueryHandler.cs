using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Invoice.GetInvoice
{
    public class GetInvoiceQueryHandler : IRequestHandler<GetInvoiceQuery, InvoiceDto>
    {
        private readonly IApplicationDbContext _context;

        public GetInvoiceQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceDto> Handle(GetInvoiceQuery request, CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(
                    i => i.InvoiceId == request.InvoiceId && i.SMEId == request.SMEId,
                    cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            return new InvoiceDto
            {
                InvoiceId = invoice.InvoiceId,
                SMEId = invoice.SMEId,
                InvoiceNumber = invoice.InvoiceNumber,
                DebtorName = invoice.DebtorName,
                DebtorContactDetails = invoice.DebtorContactDetails,
                Amount = invoice.Amount,
                IssueDate = invoice.IssueDate,
                DueDate = invoice.DueDate,
                Status = invoice.Status,
                ProcessingComplete = invoice.ProcessingComplete,
                ReviewNotes = invoice.ReviewNotes
            };
        }
    }
}
