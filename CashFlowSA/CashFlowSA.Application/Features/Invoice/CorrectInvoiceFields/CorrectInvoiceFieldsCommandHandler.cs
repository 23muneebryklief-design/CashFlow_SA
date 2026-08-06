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
                .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            // ASSUMPTION: fields can only be corrected while the invoice is still
            // Draft (i.e. before the SME has explicitly submitted it for review).
            // Adjust if the SRS intends corrections to be allowed at other stages too.
            if (invoice.Status != InvoiceStatus.Draft)
                throw new ConflictException("Only invoices in Draft status can have their fields corrected.");

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
