using CashFlowSA.Models.enums;

namespace CashFlowSA.Models.DocumentManagement
{
    public class NotificationHistory
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