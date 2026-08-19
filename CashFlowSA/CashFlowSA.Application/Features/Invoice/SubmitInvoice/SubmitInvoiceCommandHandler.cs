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

            // KYC must still be Verified at the moment the invoice enters the
            // review workflow. Do not rely on the fact that the invoice was
            // originally uploaded while KYC was verified.
            var kycStatus = await _context.KYCApplications
                .Where(k => k.SMEId == invoice.SMEId)
                .OrderByDescending(k => k.ApplicationDate)
                .Select(k => (KycStatus?)k.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (kycStatus != KycStatus.Verified)
                throw new ForbiddenException("SME must have a Verified KYC status before submitting an invoice.");

            // Draft covers the normal first-time submission path.
            // Rejected is included so an SME can resubmit after fixing
            // issues flagged during review.
            if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Rejected)
                throw new ConflictException("Only Draft or Rejected invoices can be submitted.");

            // Required fields must be completed before submission.
            if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber) || invoice.Amount <= 0)
                throw new ConflictException("Invoice fields must be completed before submission.");

            invoice.Status = InvoiceStatus.Submitted;

            // Clear any previous review so the operations queue does not
            // show a stale rejection note against the new submission.
            invoice.ReviewedByUserId = null;
            invoice.ReviewedAt = null;
            invoice.ReviewNotes = null;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}