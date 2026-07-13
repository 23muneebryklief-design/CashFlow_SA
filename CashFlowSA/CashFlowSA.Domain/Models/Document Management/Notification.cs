using CashFlowSA.Models.enums;

namespace CashFlowSA.Models.DocumentManagement
{
    public class Notification
    {
        public Guid NotificationId { get; set; }

        public Guid UserId { get; set; }

        public NotificationEvent Event { get; set; }
        public NotificationChannel Channel { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }
    }
}