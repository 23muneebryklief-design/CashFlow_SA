using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Invoice.CorrectInvoiceFields
{
    public class CorrectInvoiceFieldsCommandHandler : IRequestHandler<CorrectInvoiceFieldsCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public CorrectInvoiceFieldsCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(CorrectInvoiceFieldsCommand request, CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(
                    i => i.InvoiceId == request.InvoiceId && i.SMEId == request.SMEId,
                    cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            var kycStatus = await _context.KYCApplications
                .Where(k => k.SMEId == invoice.SMEId)
                .OrderByDescending(k => k.ApplicationDate)
                .Select(k => (KycStatus?)k.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (kycStatus != KycStatus.Verified)
                throw new ForbiddenException("SME must have a Verified KYC status before correcting invoice fields.");

            // Fields can only be corrected while the invoice is Draft
            // or Rejected. Rejected allows an SME to fix issues flagged
            // during review and resubmit the invoice.
            if (invoice.Status != InvoiceStatus.Draft && invoice.Status != InvoiceStatus.Rejected)
                throw new ConflictException("Only Draft or Rejected invoices can have their fields corrected.");

            invoice.InvoiceNumber = request.InvoiceNumber;
            invoice.DebtorName = request.DebtorName;
            invoice.DebtorContactDetails = request.DebtorContactDetails;
            invoice.Amount = request.Amount;
            invoice.IssueDate = request.IssueDate;
            invoice.DueDate = request.DueDate;

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}