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
            // Confirm the SME actually exists before we do anything else.
            var sme = await _context.SMEs
                .FirstOrDefaultAsync(s => s.SMEId == request.SMEId, cancellationToken);

            if (sme is null)
                throw new NotFoundException("SME not found.");

            // Business rule (SRS 5.2): a resubmission is only allowed if the
            // most recent application was Rejected. Pending or Verified blocks a new one.
            var existingApplication = await _context.KYCApplications
                .Where(k => k.SMEId == request.SMEId)
                .OrderByDescending(k => k.ApplicationDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingApplication is not null && existingApplication.Status != KycStatus.Rejected)
                throw new ConflictException("An active KYC application already exists for this SME.");

            // Create the application record.
            var application = new KYCApplication
            {
                ApplicationId = Guid.NewGuid(),
                SMEId = request.SMEId,
                ApplicationDate = DateTime.UtcNow,
                Status = KycStatus.Pending
            };

            _context.KYCApplications.Add(application);

            // Create one document record per uploaded document.
            // KYCDocuments links to UserId, not SMEId, so we resolve it via the SME we already loaded.
            foreach (var doc in request.Documents)
            {
                _context.KYCDocuments.Add(new KYCDocuments
                {
                    DocumentId = Guid.NewGuid(),
                    UserId = sme.UserId,
                    KYCApplicationId = application.ApplicationId,
                    DocumentType = doc.DocumentType,
                    FileName = doc.FileName,
                    FilePath = doc.FilePath,
                    FileSize = doc.FileSize,
                    UploadedAt = DateTime.UtcNow,
                    Status = DocumentStatus.Pending
                });
            }

            await _context.SaveChangesAsync(cancellationToken);

            return application.ApplicationId;
        }
    }
}