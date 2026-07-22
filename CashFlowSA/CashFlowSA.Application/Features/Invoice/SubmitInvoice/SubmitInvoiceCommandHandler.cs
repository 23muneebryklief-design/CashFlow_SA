using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Invoice.SubmitInvoice
{
    public class SubmitInvoiceCommandHandler : IRequestHandler<SubmitInvoiceCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public SubmitInvoiceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(SubmitInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Draft)
                throw new ConflictException("Only Draft invoices can be submitted.");

            // ASSUMPTION: required fields must be filled in (via CorrectInvoiceFields
            // or OCR, once built) before submission is allowed. Basic guard for now.
            if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber) || invoice.Amount <= 0)
                throw new ConflictException("Invoice fields must be completed before submission.");

            invoice.Status = InvoiceStatus.Submitted;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
