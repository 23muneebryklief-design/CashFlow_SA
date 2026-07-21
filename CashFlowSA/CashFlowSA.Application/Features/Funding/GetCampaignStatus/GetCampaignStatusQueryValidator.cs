using FluentValidation;

namespace CashFlowSA.Application.Features.Funding.GetCampaignStatus
{
    public class GetCampaignStatusQueryValidator : AbstractValidator<GetCampaignStatusQuery>
    {
        public GetCampaignStatusQueryValidator()
        {
            RuleFor(x => x.CampaignId).NotEmpty().WithMessage("Campaign ID is required.");
        }
    }
}
