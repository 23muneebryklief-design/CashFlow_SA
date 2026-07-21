using FluentValidation;

namespace CashFlowSA.Application.Features.Settlement.TriggerSettlement
{
    public class TriggerSettlementCommandValidator : AbstractValidator<TriggerSettlementCommand>
    {
        public TriggerSettlementCommandValidator()
        {
            RuleFor(x => x.CampaignId).NotEmpty();
            RuleFor(x => x.SettledAmount).GreaterThan(0);
            RuleFor(x => x.PaymentProvider).NotEmpty().MaximumLength(50);
            RuleFor(x => x.ReferenceNumber).NotEmpty().MaximumLength(100);
        }
    }
}
