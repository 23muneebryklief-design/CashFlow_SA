using FluentValidation;

namespace CashFlowSA.Application.Features.Analytics.GetFundingVolume
{
    public class GetFundingVolumeQueryValidator : AbstractValidator<GetFundingVolumeQuery>
    {
        public GetFundingVolumeQueryValidator()
        {
            RuleFor(x => x)
                .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
                .WithMessage("FromDate must not be after ToDate.")
                .WithName("DateRange");
        }
    }
}
