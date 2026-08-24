using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using CashFlowSA.Application.Common.Auditing;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Kyc.GetKycDocumentDownloadUrl
{
    public class GetKycDocumentDownloadUrlQueryHandler
        : IRequestHandler<GetKycDocumentDownloadUrlQuery, KycDocumentDownloadUrlDto>
    {
        private static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(10);

        private readonly IApplicationDbContext _context;
        private readonly IFileStorage _fileStorage;
        private readonly IAuditService _auditService;

        public GetKycDocumentDownloadUrlQueryHandler(IApplicationDbContext context, IFileStorage fileStorage, IAuditService auditService)
        {
            _context = context;
            _fileStorage = fileStorage;
            _auditService = auditService;
        }

        public async Task<KycDocumentDownloadUrlDto> Handle(
            GetKycDocumentDownloadUrlQuery request,
            CancellationToken cancellationToken)
        {
            var document = await _context.KYCDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    d => d.DocumentId == request.DocumentId && d.UserId == request.UserId,
                    cancellationToken);

            if (document is null)
                throw new NotFoundException("KYC document not found.");

            var expiresAt = DateTime.UtcNow.Add(LinkLifetime);
            var url = await _fileStorage.GetDownloadUrlAsync(
                document.FilePath,
                LinkLifetime,
                cancellationToken);

            await _auditService.RecordAsync(
                AuditAction.DownloadedDocument,
                "KYCDocuments",
                document.DocumentId,
                newValue: new { document.FileName, document.KYCApplicationId },
                cancellationToken: cancellationToken);

            return new KycDocumentDownloadUrlDto
            {
                Url = url,
                ExpiresAt = expiresAt
            };
        }
    }
}
