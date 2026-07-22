using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Settlement.GetSettlement
{
    public class SettlementDto
    {
        public Guid SettlementId { get; set; }
        public Guid CampaignId { get; set; }
        public decimal SettledAmount { get; set; }
        public SettlementStatus Status { get; set; }
        public string PaymentProvider { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public DateTime SettlementDate { get; set; }
    }
}
