using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Features.Notification.GetNotificationHistory
{
    public class NotificationDto
    {
        public Guid NotificationId { get; set; }
        public NotificationEvent Event { get; set; }
        public NotificationChannel Channel { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
