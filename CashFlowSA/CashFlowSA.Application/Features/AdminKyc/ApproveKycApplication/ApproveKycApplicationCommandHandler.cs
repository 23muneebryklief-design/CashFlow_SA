using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.AdminKyc.ApproveKycApplication
{
    // Ops-portal counterpart to SubmitKycApplicationCommand. ReviewerId should
    // be a Credit Analyst / Admin -- role enforcement itself is expected at the
    // controller via [Authorize(Roles = "CreditAnalyst,Admin")] once wired.
    public class ApproveKycApplicationCommandHandler : IRequestHandler<ApproveKycApplicationCommand, Unit>
    {
        private readonly IApplicationDbContext _context;

        public ApproveKycApplicationCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(ApproveKycApplicationCommand request, CancellationToken cancellationToken)
        {
            var application = await _context.KYCApplications
                .FirstOrDefaultAsync(a => a.ApplicationId == request.ApplicationId, cancellationToken);

            if (application is null)
                throw new NotFoundException("KYC application not found.");

            if (application.Status != KycStatus.Pending)
                throw new ConflictException("Only Pending applications can be approved.");

            application.Status = KycStatus.Verified;
            application.ReviewedAt = DateTime.UtcNow;

            _context.KYCReviews.Add(new KYCReview
            {
                Id = Guid.NewGuid(),
                KYCApplicationId = application.ApplicationId,
                ReviewerId = request.ReviewerId,
                Outcome = ReviewOutcome.Approved,
                Notes = request.Notes,
                ReviewDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
