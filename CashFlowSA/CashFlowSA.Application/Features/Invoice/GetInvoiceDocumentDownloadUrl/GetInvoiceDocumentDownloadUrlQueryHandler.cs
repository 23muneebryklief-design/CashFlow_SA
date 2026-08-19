using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Invoice.GetInvoiceDocumentDownloadUrl
{
    public class GetInvoiceDocumentDownloadUrlQueryHandler
        : IRequestHandler<GetInvoiceDocumentDownloadUrlQuery, InvoiceDocumentDownloadUrlDto>
    {
        private static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(10);
        private readonly IApplicationDbContext _context;
        private readonly IFileStorage _fileStorage;

        public GetInvoiceDocumentDownloadUrlQueryHandler(IApplicationDbContext context, IFileStorage fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<InvoiceDocumentDownloadUrlDto> Handle(
            GetInvoiceDocumentDownloadUrlQuery request,
            CancellationToken cancellationToken)
        {
            var invoice = await _context.Invoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceId == request.InvoiceId, cancellationToken);

            if (invoice is null)
                throw new NotFoundException("Invoice not found.");

            if (string.Equals(request.Role, "SME", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.SmeId.HasValue || invoice.SMEId != request.SmeId.Value)
                    throw new ForbiddenException("You may only access documents belonging to your own SME profile.");
            }
            else if (string.Equals(request.Role, "CreditAnalyst", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(request.Role, "Admin", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(request.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (invoice.Status == InvoiceStatus.Draft)
                    throw new ForbiddenException("Draft invoice documents are not available for reviewer access.");
            }
            else
            {
                throw new ForbiddenException("You are not authorized to access invoice documents.");
            }

            var document = await _context.InvoiceDocuments
                .AsNoTracking()
                .Where(d => d.InvoiceId == invoice.InvoiceId)
                .OrderByDescending(d => d.UploadedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (document is null)
                throw new NotFoundException("Invoice document not found.");

            var expiresAt = DateTime.UtcNow.Add(LinkLifetime);
            var url = await _fileStorage.GetDownloadUrlAsync(
                document.FilePath,
                LinkLifetime,
                cancellationToken);

            return new InvoiceDocumentDownloadUrlDto
            {
                Url = url,
                ExpiresAt = expiresAt,
                FileName = document.FileName
            };
        }
    }
}
