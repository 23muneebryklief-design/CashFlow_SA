using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Invoice.UploadInvoice
{
    public class UploadInvoiceCommandHandler : IRequestHandler<UploadInvoiceCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public UploadInvoiceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(UploadInvoiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Confirm the SME exists.
            var sme = await _context.SMEs
                .FirstOrDefaultAsync(s => s.SMEId == request.SMEId, cancellationToken);

            if (sme is null)
                throw new NotFoundException("SME not found.");

            // 2. Confirm KYC is Verified (SRS 5.2 AC: non-Verified users are blocked
            // from transactional actions). Look up the most recent application.
            var kycApplication = await _context.KYCApplications
                .Where(k => k.SMEId == request.SMEId)
                .OrderByDescending(k => k.ApplicationDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (kycApplication is null || kycApplication.Status != KycStatus.Verified)
                throw new ForbiddenException("SME must have a Verified KYC status before uploading an invoice.");

            // 3. Create the Invoice with placeholder values.
            // The invoice number is unique while the record is still a draft
            // because the database enforces uniqueness on InvoiceNumber.
            // OCR (not yet built) or manual correction will replace it later.
            var invoiceId = Guid.NewGuid();
            var invoice = new Domain.Models.Invoice
            {
                InvoiceId = invoiceId,
                SMEId = request.SMEId,
                InvoiceNumber = $"DRAFT-{invoiceId:N}",
                DebtorName = string.Empty,
                DebtorContactDetails = string.Empty,
                Amount = 0,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow,
                Status = InvoiceStatus.Draft,
                ProcessingComplete = false
            };

            _context.Invoices.Add(invoice);

            // 4. Create the InvoiceDocument backing this invoice.
            var invoiceDocument = new InvoiceDocument
            {
                InvoiceDocumentId = Guid.NewGuid(),
                InvoiceId = invoice.InvoiceId,
                FileName = request.FileName,
                FilePath = request.FilePath,
                FileSize = request.FileSize,
                Status = DocumentStatus.Pending,
                UploadedAt = DateTime.UtcNow
            };

            _context.InvoiceDocuments.Add(invoiceDocument);

            // 5. Save and return the new invoice's ID.
            await _context.SaveChangesAsync(cancellationToken);

            return invoice.InvoiceId;
        }
    }
}