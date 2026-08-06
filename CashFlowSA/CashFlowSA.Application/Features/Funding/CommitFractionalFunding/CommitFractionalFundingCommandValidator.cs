using FluentValidation;

namespace CashFlowSA.Application.Features.Funding.CommitFractionalFunding
{
    public class CommitFractionalFundingCommandValidator : AbstractValidator<CommitFractionalFundingCommand>
    {
        public CommitFractionalFundingCommandValidator()
        {
            RuleFor(x => x.CampaignId).NotEmpty();
            RuleFor(x => x.InvestorId).NotEmpty();
            RuleFor(x => x.Amount).GreaterThan(0);
        }
    }
}
