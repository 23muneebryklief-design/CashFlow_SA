using MediatR;

namespace CashFlowSA.Application.Features.Settlement.TriggerSettlement
{
    public class TriggerSettlementCommand : IRequest<Guid>
    {
        public Guid CampaignId { get; set; }

        // Total amount the debtor paid (simulated -- SRS 2.2, always sandbox).
        public decimal SettledAmount { get; set; }

        public string PaymentProvider { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
    }
}
