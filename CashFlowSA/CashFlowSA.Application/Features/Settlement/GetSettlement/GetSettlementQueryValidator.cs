using FluentValidation;

namespace CashFlowSA.Application.Features.Settlement.GetSettlement
{
    public class GetSettlementQueryValidator : AbstractValidator<GetSettlementQuery>
    {
        public GetSettlementQueryValidator()
        {
            RuleFor(x => x.SettlementId).NotEmpty().WithMessage("Settlement ID is required.");
        }
    }
}
