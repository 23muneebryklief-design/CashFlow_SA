using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Settlement.GetSettlement
{
    public class GetSettlementQueryHandler : IRequestHandler<GetSettlementQuery, SettlementDto>
    {
        private readonly IApplicationDbContext _context;

        public GetSettlementQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SettlementDto> Handle(GetSettlementQuery request, CancellationToken cancellationToken)
        {
            var settlement = await _context.Settlements
                .FirstOrDefaultAsync(s => s.SettlementId == request.SettlementId, cancellationToken);

            if (settlement is null)
                throw new NotFoundException("Settlement not found.");

            return new SettlementDto
            {
                SettlementId = settlement.SettlementId,
                CampaignId = settlement.CampaignId,
                SettledAmount = settlement.SettledAmount,
                Status = settlement.Status,
                PaymentProvider = settlement.PaymentProvider,
                ReferenceNumber = settlement.ReferenceNumber,
                SettlementDate = settlement.SettlementDate
            };
        }
    }
}
