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

    Task BroadcastFundingUpdateAsync(
        Guid campaignId,
        Guid invoiceId,
        decimal targetAmount,
        decimal fundedAmount,
        CampaignStatus status,
        CancellationToken cancellationToken = default);
}
