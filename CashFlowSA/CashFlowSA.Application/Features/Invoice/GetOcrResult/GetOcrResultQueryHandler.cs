using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Invoice.GetOcrResult
{
    public sealed class GetOcrResultQueryHandler : IRequestHandler<GetOcrResultQuery, OcrResultDto>
    {
        private readonly IApplicationDbContext _context;

        public GetOcrResultQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OcrResultDto> Handle(
            GetOcrResultQuery request,
            CancellationToken cancellationToken)
        {
            var invoiceExists = await _context.Invoices
                .AsNoTracking()
                .AnyAsync(
                    i => i.InvoiceId == request.InvoiceId && i.SMEId == request.SMEId,
                    cancellationToken);

            if (!invoiceExists)
                throw new NotFoundException("Invoice not found.");

            var result = await _context.OCRResults
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.InvoiceId == request.InvoiceId,
                    cancellationToken);

            if (result is null)
                throw new NotFoundException(
                    "OCR processing has not produced a result for this invoice yet.");

            return new OcrResultDto
            {
                InvoiceId = result.InvoiceId,
                InvoiceNumber = result.ExtractedInvoiceNumber,
                Amount = result.ExtractedAmount,
                DueDate = result.ExtractedDueDate,
                DebtorName = result.ExtractedDebtorName,
                ConfidenceScore = result.ConfidenceScore,
                RequiresManualReview = result.RequiresManualReview,
                ProcessedAt = result.ProcessedAt
            };
        }
    }
}
