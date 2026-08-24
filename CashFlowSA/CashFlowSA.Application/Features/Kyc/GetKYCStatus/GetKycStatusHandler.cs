using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Features.Kyc.DTO;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Kyc.GetKYCStatus
{
    public class GetKycStatusQueryHandler
        : IRequestHandler<GetKycStatusQuery, KycStatusDto>
    {
        private readonly IApplicationDbContext _context;

        public GetKycStatusQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<KycStatusDto> Handle(
            GetKycStatusQuery request,
            CancellationToken cancellationToken)
        {
            var sme = await _context.SMEs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.SMEId == request.SMEId,
                    cancellationToken);

            if (sme is null)
                throw new NotFoundException("SME not found.");

            var application = await _context.KYCApplications
                .AsNoTracking()
                .Where(x => x.SMEId == request.SMEId)
                .OrderByDescending(x => x.ApplicationDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (application is null)
                throw new NotFoundException(
                    "No KYC application found for this SME.");

            var documents = await _context.KYCDocuments
                .AsNoTracking()
                .Where(x => x.KYCApplicationId == application.ApplicationId)
                .OrderBy(x => x.DocumentType)
                .Select(x => new KycDocumentStatusDto
                {
                    DocumentType = x.DocumentType,
                    FileName = x.FileName,
                    Status = x.Status,
                    UploadedAt = x.UploadedAt
                })
                .ToListAsync(cancellationToken);

            var review = await _context.KYCReviews
                .AsNoTracking()
                .Where(x => x.KYCApplicationId == application.ApplicationId)
                .OrderByDescending(x => x.ReviewDate)
                .FirstOrDefaultAsync(cancellationToken);

            return new KycStatusDto
            {
                ApplicationId = application.ApplicationId,
                Status = application.Status,
                ApplicationDate = application.ApplicationDate,
                ReviewedAt = application.ReviewedAt,
                ReviewOutcome = review?.Outcome.ToString(),
                ReviewNotes = review?.Notes,
                Documents = documents
            };
        }
    }
}