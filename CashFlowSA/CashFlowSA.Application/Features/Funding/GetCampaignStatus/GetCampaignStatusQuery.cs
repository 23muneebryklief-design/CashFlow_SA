using MediatR;

namespace CashFlowSA.Application.Features.Funding.GetCampaignStatus
{
    public class GetCampaignStatusQuery : IRequest<CampaignStatusDto>
    {
        public Guid CampaignId { get; set; }
    }
}
