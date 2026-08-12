using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Features.Kyc.DTO;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Kyc.Queries.GetKycStatus
{
    public class GetKycStatusQueryHandler : IRequestHandler<GetKycStatusQuery, KycStatusDto>
    {
        private readonly IApplicationDbContext _context;

        public GetKycStatusQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<KycStatusDto> Handle(GetKycStatusQuery request, CancellationToken cancellationToken)
        {
            var sme = await _context.SMEs
                .FirstOrDefaultAsync(s => s.SMEId == request.SMEId, cancellationToken);

            if (sme is null)
                throw new NotFoundException("SME not found.");

            var application = await _context.KYCApplications
                .Where(k => k.SMEId == request.SMEId)
                .OrderByDescending(k => k.ApplicationDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (application is null)
                throw new NotFoundException("No KYC application found for this SME.");

            // KYCDocuments currently links to UserId rather than KYCApplicationId.
            // Submission creates the document rows immediately after the application,
            // so UploadedAt gives us a safe boundary for the latest submission.
            var documents = await _context.KYCDocuments
                .Where(d => d.UserId == sme.UserId && d.UploadedAt >= application.ApplicationDate)
                .OrderBy(d => d.DocumentType)
                .Select(d => new KycDocumentStatusDto
                {
                    DocumentType = d.DocumentType,
                    FileName = d.FileName,
                    Status = application.Status == KycStatus.Verified
                        ? DocumentStatus.Approved
                        : application.Status == KycStatus.Rejected
                            ? DocumentStatus.Rejected
                            : d.Status,
                    UploadedAt = d.UploadedAt
                })
                .ToListAsync(cancellationToken);

            return new KycStatusDto
            {
                ApplicationId = application.ApplicationId,
                Status = application.Status,
                ApplicationDate = application.ApplicationDate,
                Documents = documents
            };
        }
    }
}
