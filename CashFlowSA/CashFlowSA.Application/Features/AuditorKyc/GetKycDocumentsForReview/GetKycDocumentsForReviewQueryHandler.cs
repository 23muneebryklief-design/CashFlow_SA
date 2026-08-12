using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Features.AuditorKyc.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.AuditorKyc.GetKycDocumentsForReview
{
    public class GetKycDocumentsForReviewQueryHandler
        : IRequestHandler<GetKycDocumentsForReviewQuery, List<SmeKycReviewSectionDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetKycDocumentsForReviewQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SmeKycReviewSectionDto>> Handle(
            GetKycDocumentsForReviewQuery request,
            CancellationToken cancellationToken)
        {
            var documentsQuery = _context.KYCDocuments.AsQueryable();

            if (request.StatusFilter.HasValue)
                documentsQuery = documentsQuery.Where(d => d.Status == request.StatusFilter.Value);

            // Only SMEs that actually have documents matching the filter show up
            // as a section -- an auditor reviewing "Pending" shouldn't see empty
            // sections for SMEs whose documents were already actioned.
            var documentsByUser = await documentsQuery
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync(cancellationToken);

            var userIds = documentsByUser.Select(d => d.UserId).Distinct().ToList();

            // Latest application per SME, so the auditor has application-level
            // context (e.g. "already Verified") alongside the raw documents.
            var latestApplications = await _context.KYCApplications
                .Where(a => userIds.Contains(a.SME.UserId))
                .GroupBy(a => a.SMEId)
                .Select(g => g.OrderByDescending(a => a.ApplicationDate).First())
                .ToListAsync(cancellationToken);

            // Only documents belonging to that latest application -- otherwise
            // a rejected-and-resubmitted SME would show old, already-actioned
            // documents mixed in alongside their new pending ones.
            var currentApplicationIds = latestApplications.Select(a => a.ApplicationId).ToHashSet();
            documentsByUser = documentsByUser
                .Where(d => d.KYCApplicationId.HasValue && currentApplicationIds.Contains(d.KYCApplicationId.Value))
                .ToList();

            var smes = await _context.SMEs
                .Where(s => userIds.Contains(s.UserId))
                .Select(s => new { s.SMEId, s.UserId, s.CompanyName, s.ContactPerson })
                .ToListAsync(cancellationToken);

            var sections = smes
                .Select(sme => new SmeKycReviewSectionDto
                {
                    SMEId = sme.SMEId,
                    UserId = sme.UserId,
                    CompanyName = sme.CompanyName,
                    ContactPerson = sme.ContactPerson,
                    ApplicationStatus = latestApplications
                        .FirstOrDefault(a => a.SMEId == sme.SMEId)?.Status,
                    Documents = documentsByUser
                        .Where(d => d.UserId == sme.UserId)
                        .Select(d => new KycDocumentReviewDto
                        {
                            DocumentId = d.DocumentId,
                            DocumentType = d.DocumentType,
                            FileName = d.FileName,
                            FileSize = d.FileSize,
                            UploadedAt = d.UploadedAt,
                            Status = d.Status,
                            ReviewedAt = d.ReviewedAt,
                            ReviewNotes = d.ReviewNotes
                        })
                        .ToList()
                })
                .OrderBy(s => s.CompanyName)
                .ToList();

            return sections;
        }
    }
}
