using FluentValidation;

namespace CashFlowSA.Application.Features.Marketplace.GetListingDetail
{
    public class GetListingDetailQueryValidator : AbstractValidator<GetListingDetailQuery>
    {
        public GetListingDetailQueryValidator()
        {
            RuleFor(x => x.ListingId).NotEmpty().WithMessage("Listing ID is required.");
        }
    }
}
