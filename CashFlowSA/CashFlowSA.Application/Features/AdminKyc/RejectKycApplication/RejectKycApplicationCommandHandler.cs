using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.AdminKyc.RejectKycApplication
{
    public class RejectKycApplicationCommandHandler : IRequestHandler<RejectKycApplicationCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public RejectKycApplicationCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(RejectKycApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _context.KYCApplications
                .FirstOrDefaultAsync(a => a.ApplicationId == request.ApplicationId, cancellationToken);

            if (application is null)
                throw new NotFoundException("KYC application not found.");

            if (application.Status != KycStatus.Pending)
                throw new ConflictException("Only Pending applications can be rejected.");

            application.Status = KycStatus.Rejected;
            application.ReviewedAt = DateTime.UtcNow;

            var sme = await _context.SMEs
                .FirstOrDefaultAsync(s => s.SMEId == application.SMEId, cancellationToken);

            if (sme is null)
                throw new NotFoundException("SME not found.");

            var applicationDocuments = await _context.KYCDocuments
                .Where(d => d.KYCApplicationId == application.ApplicationId)
                .ToListAsync(cancellationToken);

            foreach (var document in applicationDocuments)
                document.Status = DocumentStatus.Rejected;

            _context.KYCReviews.Add(new KYCReview
            {
                Id = Guid.NewGuid(),
                KYCApplicationId = application.ApplicationId,
                ReviewerId = request.ReviewerId,
                Outcome = ReviewOutcome.Rejected,
                Notes = request.Notes,
                ReviewDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
