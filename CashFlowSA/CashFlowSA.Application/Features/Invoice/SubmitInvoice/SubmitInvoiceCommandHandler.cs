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
                .FirstOrDefaultAsync(
                    i => i.InvoiceId == request.InvoiceId && i.SMEId == request.SMEId,
                    cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Rejected)
                throw new ConflictException("Only Draft or Rejected invoices can be submitted.");

            // ASSUMPTION: required fields must be filled in (via CorrectInvoiceFields
            // or OCR, once built) before submission is allowed. Basic guard for now.
            if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber) || invoice.Amount <= 0)
                throw new ConflictException("Invoice fields must be completed before submission.");

            invoice.Status = InvoiceStatus.Submitted;

            // Clear any previous review so the ops queue doesn't show a stale
            // rejection note against what is now a fresh submission.
            invoice.ReviewedByUserId = null;
            invoice.ReviewedAt = null;
            invoice.ReviewNotes = null;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
