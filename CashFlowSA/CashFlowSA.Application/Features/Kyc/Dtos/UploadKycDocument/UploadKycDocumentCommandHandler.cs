using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Features.Kyc.DTO;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Kyc.UploadKycDocument
{
    public class UploadKycDocumentCommandHandler : IRequestHandler<UploadKycDocumentCommand, UploadKycDocumentResultDto>
    {
        private readonly IFileStorage _fileStorage;
        private readonly IApplicationDbContext _context;

        public UploadKycDocumentCommandHandler(IFileStorage fileStorage, IApplicationDbContext context)
        {
            _fileStorage = fileStorage;
            _context = context;
        }

        public async Task<UploadKycDocumentResultDto> Handle(UploadKycDocumentCommand request, CancellationToken cancellationToken)
        {
            if (request.UserId == Guid.Empty)
                throw new UnauthorizedAccessException("Authenticated user could not be determined.");

            // KYC is a state machine: documents are uploaded before submission,
            // and after a rejection the SME may upload a fresh set for resubmission.
            // Once an application is Pending or Verified, do not allow loose
            // documents to accumulate outside the active application.
            var smeId = await _context.SMEs
                .Where(s => s.UserId == request.UserId)
                .Select(s => (Guid?)s.SMEId)
                .FirstOrDefaultAsync(cancellationToken);

            if (!smeId.HasValue)
                throw new ForbiddenException("Only an SME profile can upload KYC documents.");

            var latestApplicationStatus = await _context.KYCApplications
                .Where(k => k.SMEId == smeId.Value)
                .OrderByDescending(k => k.ApplicationDate)
                .Select(k => (KycStatus?)k.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (latestApplicationStatus == KycStatus.Pending)
                throw new ConflictException("A KYC application is already under review. Wait for the decision before uploading more documents.");

            if (latestApplicationStatus == KycStatus.Verified)
                throw new ConflictException("KYC is already verified. Additional KYC documents cannot be uploaded unless the application is rejected.");

            var result = await _fileStorage.UploadAsync(
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);

            // Persist the ownership record immediately. The later KYC submission
            // can only attach files that were uploaded by this same authenticated user.
            var document = new KYCDocuments
            {
                DocumentId = Guid.NewGuid(),
                UserId = request.UserId,
                FileName = result.FileName,
                FilePath = result.FilePath,
                FileSize = result.FileSize,
                UploadedAt = DateTime.UtcNow,
                Status = DocumentStatus.Pending
            };

            _context.KYCDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return new UploadKycDocumentResultDto
            {
                DocumentId = document.DocumentId,
                FileName = result.FileName,
                FilePath = result.FilePath,
                FileSize = result.FileSize
            };
        }
    }
}
