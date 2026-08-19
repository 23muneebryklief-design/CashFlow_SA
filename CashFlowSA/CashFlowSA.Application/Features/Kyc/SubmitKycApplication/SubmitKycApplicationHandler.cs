using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Kyc.SubmitKycApplication
{
    public class SubmitKycApplicationCommandHandler : IRequestHandler<SubmitKycApplicationCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public SubmitKycApplicationCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(SubmitKycApplicationCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId == Guid.Empty)
                throw new UnauthorizedAccessException("Authenticated user could not be determined.");

            var sme = await _context.SMEs
                .FirstOrDefaultAsync(s => s.SMEId == request.SMEId && s.UserId == request.UserId, cancellationToken);

            if (sme is null)
                throw new ForbiddenException("You may only submit KYC documents for your own SME profile.");

            if (request.Documents.Count == 0)
                throw new ConflictException("At least one KYC document is required.");

            var existingApplication = await _context.KYCApplications
                .Where(k => k.SMEId == request.SMEId)
                .OrderByDescending(k => k.ApplicationDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingApplication is not null && existingApplication.Status != KycStatus.Rejected)
                throw new ConflictException("An active KYC application already exists for this SME.");

            var requestedPaths = request.Documents
                .Select(d => d.FilePath)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (requestedPaths.Count != request.Documents.Count)
                throw new ConflictException("Each KYC document must reference a valid uploaded file.");

            var ownedDocuments = await _context.KYCDocuments
                .Where(d => d.UserId == request.UserId
                    && d.KYCApplicationId == null
                    && requestedPaths.Contains(d.FilePath))
                .ToListAsync(cancellationToken);

            if (ownedDocuments.Count != requestedPaths.Count)
                throw new ForbiddenException("One or more KYC files were not uploaded by the authenticated user.");

            var application = new KYCApplication
            {
                ApplicationId = Guid.NewGuid(),
                SMEId = request.SMEId,
                ApplicationDate = DateTime.UtcNow,
                Status = KycStatus.Pending
            };

            _context.KYCApplications.Add(application);

            foreach (var submittedDocument in request.Documents)
            {
                var document = ownedDocuments.First(d => d.FilePath == submittedDocument.FilePath);
                document.KYCApplicationId = application.ApplicationId;
                document.DocumentType = submittedDocument.DocumentType;
                document.FileName = submittedDocument.FileName;
                document.FileSize = submittedDocument.FileSize;
                document.Status = DocumentStatus.Pending;
                document.ReviewedByUserId = null;
                document.ReviewedAt = null;
                document.ReviewNotes = null;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return application.ApplicationId;
        }
    }
}
