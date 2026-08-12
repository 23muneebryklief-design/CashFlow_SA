using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.AuditorKyc.RejectKycDocument
{
    public class RejectKycDocumentCommandHandler : IRequestHandler<RejectKycDocumentCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public RejectKycDocumentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(RejectKycDocumentCommand request, CancellationToken cancellationToken)
        {
            var document = await _context.KYCDocuments
                .FirstOrDefaultAsync(d => d.DocumentId == request.DocumentId, cancellationToken);

            if (document is null)
                throw new NotFoundException("KYC document not found.");

            if (document.Status != DocumentStatus.Pending)
                throw new ConflictException("Only Pending documents can be rejected.");

            document.Status = DocumentStatus.Rejected;
            document.ReviewedByUserId = request.ReviewerId;
            document.ReviewedAt = DateTime.UtcNow;
            document.ReviewNotes = request.Notes;

            // Unlike approval, a single rejected document fails the whole
            // application right away -- there's no reason to wait on the
            // remaining documents once one is known to be bad. The SME sees
            // Rejected and resubmits everything together.
            if (document.KYCApplicationId.HasValue)
            {
                var application = await _context.KYCApplications
                    .FirstOrDefaultAsync(a => a.ApplicationId == document.KYCApplicationId.Value, cancellationToken);

                if (application is not null && application.Status == KycStatus.Pending)
                {
                    application.Status = KycStatus.Rejected;
                    application.ReviewedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
