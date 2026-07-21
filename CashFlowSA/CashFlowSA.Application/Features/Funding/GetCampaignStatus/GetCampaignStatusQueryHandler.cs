using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CashFlowSA.Application.Features.Funding.GetCampaignStatus
{
    public class GetCampaignStatusQueryHandler : IRequestHandler<GetCampaignStatusQuery, CampaignStatusDto>
    {
        private readonly IApplicationDbContext _context;

        public GetCampaignStatusQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CampaignStatusDto> Handle(GetCampaignStatusQuery request, CancellationToken cancellationToken)
        {
            var campaign = await _context.FundingCampaigns
                .FirstOrDefaultAsync(c => c.CampaignId == request.CampaignId, cancellationToken);

            if (campaign is null)
                throw new NotFoundException("Funding campaign not found.");

            return new CampaignStatusDto
            {
                CampaignId = campaign.CampaignId,
                Status = campaign.Status,
                FundingModel = campaign.FundingModel,
                TargetAmount = campaign.TargetAmount,
                FundedAmount = campaign.FundedAmount,
                FundingDeadline = campaign.FundingDeadline
            };
        }
    }
}
