using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.AuditorKyc.ApproveKycDocument
{
    public class ApproveKycDocumentCommandHandler : IRequestHandler<ApproveKycDocumentCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ApproveKycDocumentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ApproveKycDocumentCommand request, CancellationToken cancellationToken)
        {
            var document = await _context.KYCDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId, cancellationToken);

            if (document is null)
                throw new NotFoundException("KYC document not found.");

            if (document.Status != DocumentStatus.Pending)
                throw new ConflictException("Only Pending documents can be approved.");

            document.Status = DocumentStatus.Approved;
            document.ReviewedByUserId = request.ReviewerId;
            document.ReviewedAt = DateTime.UtcNow;
            document.ReviewNotes = request.Notes;

            // If this was the last Pending document on its application, the
            // application is done -- verify it automatically rather than
            // requiring a separate manual sign-off step.
            if (document.KYCApplicationId.HasValue)
            {
                var application = await _context.KYCApplications
                    .FirstOrDefaultAsync(a => a.ApplicationId == document.KYCApplicationId.Value, cancellationToken);

                if (application is not null && application.Status == KycStatus.Pending)
                {
                    var stillPending = await _context.KYCDocuments
                        .AnyAsync(
                            d => d.KYCApplicationId == application.ApplicationId
                                && d.DocumentId != document.DocumentId
                                && d.Status == DocumentStatus.Pending,
                            cancellationToken);

                    if (!stillPending)
                    {
                        application.Status = KycStatus.Verified;
                        application.ReviewedAt = DateTime.UtcNow;
                    }
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
