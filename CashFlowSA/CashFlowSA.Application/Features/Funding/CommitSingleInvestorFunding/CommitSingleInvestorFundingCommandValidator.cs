using FluentValidation;

namespace CashFlowSA.Application.Features.Funding.CommitSingleInvestorFunding
{
    public class CommitSingleInvestorFundingCommandValidator : AbstractValidator<CommitSingleInvestorFundingCommand>
    {
        public CommitSingleInvestorFundingCommandValidator()
        {
            RuleFor(x => x.CampaignId).NotEmpty();
            RuleFor(x => x.InvestorId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
