using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Domain.Models
{
    public class NotificationHistory : BaseEntity
    {
        public Guid HistoryId { get; set; }

        public Guid NotificationId { get; set; }

        public NotificationChannel Channel { get; set; }

        public NotificationDeliveryStatus DeliveryStatus { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeliveredAt { get; set; }

        public string? FailureReason { get; set; }
    }
}