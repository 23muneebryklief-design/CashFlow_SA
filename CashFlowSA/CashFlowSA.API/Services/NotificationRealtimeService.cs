using CashFlowSA.API.Hubs;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using Microsoft.AspNetCore.SignalR;

namespace CashFlowSA.API.Services;

public interface INotificationRealtimeService
{
    Task NotifyAsync(
        Notification notification,
        CancellationToken cancellationToken = default);

    Task BroadcastFundingUpdateAsync(
        Guid campaignId,
        Guid invoiceId,
        decimal targetAmount,
        decimal fundedAmount,
        CampaignStatus status,
        CancellationToken cancellationToken = default);
}

public sealed class NotificationRealtimeService : INotificationRealtimeService
{
    private const string ClientEvent = "notificationReceived";

    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationRealtimeService(
        IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task BroadcastFundingUpdateAsync(
        Guid campaignId,
        Guid invoiceId,
        decimal targetAmount,
        decimal fundedAmount,
        CampaignStatus status,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            campaignId,
            invoiceId,
            targetAmount,
            fundedAmount,
            status = status.ToString()
        };

        return _hubContext.Clients
            .All
            .SendAsync(
                "fundingUpdated",
                payload,
                cancellationToken);
    }

    public Task NotifyAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            notificationId = notification.NotificationId,
            @event = notification.Event.ToString(),
            channel = notification.Channel.ToString(),
            title = notification.Title,
            message = notification.Message,
            isRead = notification.IsRead,
            readAt = notification.ReadAt,
            createdAt = notification.CreatedAt
        };

        return _hubContext.Clients
            .Group(NotificationHub.GetGroupName(notification.UserId))
            .SendAsync(
                ClientEvent,
                payload,
                cancellationToken);
    }
}