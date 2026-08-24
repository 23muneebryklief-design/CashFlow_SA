using CashFlowSA.Domain.Models.Enums;

namespace CashFlowSA.Application.Common.Notifications;

public interface INotificationDispatcher
{
    Task<Guid> DispatchAsync(
        Guid userId,
        NotificationEvent notificationEvent,
        string title,
        string message,
        IReadOnlyCollection<NotificationChannel> channels,
        CancellationToken cancellationToken = default);
}
