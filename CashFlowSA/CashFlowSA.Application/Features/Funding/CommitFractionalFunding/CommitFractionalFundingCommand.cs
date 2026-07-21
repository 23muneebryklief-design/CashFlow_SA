using MediatR;

namespace CashFlowSA.Application.Features.Funding.CommitFractionalFunding
{
    public class CommitFractionalFundingCommand : IRequest<Guid>
    {
        public Guid CampaignId { get; set; }
        public Guid InvestorId { get; set; }
        public decimal Amount { get; set; }
    }
}
