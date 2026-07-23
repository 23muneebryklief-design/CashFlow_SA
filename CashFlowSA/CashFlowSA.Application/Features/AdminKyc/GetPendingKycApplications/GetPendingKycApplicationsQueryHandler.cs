using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Domain.Models.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.AdminKyc.GetPendingKycApplications
{
    public class GetPendingKycApplicationsQueryHandler : IRequestHandler<GetPendingKycApplicationsQuery, List<PendingKycApplicationDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetPendingKycApplicationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<PendingKycApplicationDto>> Handle(GetPendingKycApplicationsQuery request, CancellationToken cancellationToken)
        {
            // Oldest-first: a review queue should work through the longest-waiting
            // applications first, not the most recently submitted.
            return await _context.KYCApplications
                .Where(a => a.Status == KycStatus.Pending)
                .OrderBy(a => a.ApplicationDate)
                .Select(a => new PendingKycApplicationDto
                {
                    ApplicationId = a.ApplicationId,
                    SMEId = a.SMEId,
                    ApplicationDate = a.ApplicationDate
                })
                .ToListAsync(cancellationToken);
        }
    }
}
