using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.InvoiceReview.ApproveInvoice
{
    public class ApproveInvoiceCommandHandler : IRequestHandler<ApproveInvoiceCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ApproveInvoiceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ApproveInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            // SRS 5.3 status flow includes an UnderReview step, but nothing in this
            // codebase currently transitions Submitted -> UnderReview as a separate
            // action (no ops "claim this invoice" step exists yet). Approve/Reject
            // act directly on Submitted for now -- see the KYC document review
            // pattern, which similarly treats "Pending" as the whole review queue.
            if (invoice.Status != InvoiceStatus.Submitted)
                throw new ConflictException("Only Submitted invoices can be approved.");

            invoice.Status = InvoiceStatus.Approved;
            invoice.ReviewedByUserId = request.ReviewerId;
            invoice.ReviewedAt = DateTime.UtcNow;
            invoice.ReviewNotes = request.Notes;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
