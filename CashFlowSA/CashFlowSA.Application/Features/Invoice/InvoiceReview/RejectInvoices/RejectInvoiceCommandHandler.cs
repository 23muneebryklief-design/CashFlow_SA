using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.InvoiceReview.RejectInvoice
{
    public class RejectInvoiceCommandHandler : IRequestHandler<RejectInvoiceCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public RejectInvoiceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(RejectInvoiceCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            if (invoice.Status != InvoiceStatus.Submitted)
                throw new ConflictException("Only Submitted invoices can be rejected.");

            invoice.Status = InvoiceStatus.Rejected;
            invoice.ReviewedByUserId = request.ReviewerId;
            invoice.ReviewedAt = DateTime.UtcNow;
            invoice.ReviewNotes = request.Notes;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
