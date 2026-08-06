using MediatR;

namespace CashFlowSA.Application.Features.Funding.CommitSingleInvestorFunding
{
    public class CommitSingleInvestorFundingCommand : IRequest<Guid>
    {
        public Guid CampaignId { get; set; }
        public Guid InvestorId { get; set; }
        public decimal Amount { get; set; }
    }
}
