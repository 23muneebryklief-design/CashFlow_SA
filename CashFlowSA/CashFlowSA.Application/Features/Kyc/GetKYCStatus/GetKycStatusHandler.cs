using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using CashFlowSA.Application.Features.Kyc.DTO;
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

            return new KycStatusDto
            {
                ApplicationId = application.ApplicationId,
                Status = application.Status,
                ApplicationDate = application.ApplicationDate
            };
        }
    }
}
