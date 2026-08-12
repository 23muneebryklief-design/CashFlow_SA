using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.AuditorKyc.GetKycDocumentDownloadUrl
{
    public class GetKycDocumentDownloadUrlQueryHandler
        : IRequestHandler<GetKycDocumentDownloadUrlQuery, KycDocumentDownloadUrlDto>
    {
        private static readonly TimeSpan LinkLifetime = TimeSpan.FromMinutes(10);

        private readonly IApplicationDbContext _context;
        private readonly IFileStorage _fileStorage;

        public GetKycDocumentDownloadUrlQueryHandler(IApplicationDbContext context, IFileStorage fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        public async Task<KycDocumentDownloadUrlDto> Handle(
            GetKycDocumentDownloadUrlQuery request,
            CancellationToken cancellationToken)
        {
            var document = await _context.KYCDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId, cancellationToken);

            if (document is null)
                throw new NotFoundException("KYC document not found.");

            var url = await _fileStorage.GetDownloadUrlAsync(document.FilePath, LinkLifetime, cancellationToken);

            return new KycDocumentDownloadUrlDto
            {
                Url = url,
                ExpiresAt = DateTime.UtcNow.Add(LinkLifetime)
            };
        }
    }
}
