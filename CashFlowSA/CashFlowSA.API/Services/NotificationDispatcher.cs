using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Notifications;
using CashFlowSA.Application.Common.Settings;
using CashFlowSA.Domain.Models;
using CashFlowSA.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CashFlowSA.API.Services;

public sealed class NotificationDispatcher : INotificationDispatcher
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailNotificationSender _email;
    private readonly ISmsNotificationSender _sms;
    private readonly NotificationDeliverySettings _settings;
    private readonly ILogger<NotificationDispatcher> _logger;
    private readonly INotificationRealtimeService _realtime;

    public NotificationDispatcher(
        IApplicationDbContext db,
        IEmailNotificationSender email,
        ISmsNotificationSender sms,
        IOptions<NotificationDeliverySettings> options,
        ILogger<NotificationDispatcher> logger,
        INotificationRealtimeService realtime)
    {
        _db = db;
        _email = email;
        _sms = sms;
        _settings = options.Value;
        _logger = logger;
        _realtime = realtime;
    }

    public async Task<Guid> DispatchAsync(
        Guid userId,
        NotificationEvent notificationEvent,
        string title,
        string message,
        IReadOnlyCollection<NotificationChannel> channels,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("A valid user ID is required.", nameof(userId));

        var user = await _db.Users.SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User '{userId}' was not found.");

        var selectedChannels = channels.Distinct().ToArray();
        if (selectedChannels.Length == 0)
            throw new ArgumentException("At least one notification channel is required.", nameof(channels));

        var notificationId = Guid.NewGuid();
        var notification = new Notification
        {
            NotificationId = notificationId,
            UserId = userId,
            Event = notificationEvent,
            Channel = selectedChannels.Contains(NotificationChannel.InApp)
                ? NotificationChannel.InApp
                : selectedChannels[0],
            Title = title,
            Message = message,
            IsRead = false
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        if (selectedChannels.Contains(NotificationChannel.InApp))
        {
            var inAppHistory = new NotificationHistory
            {
                HistoryId = Guid.NewGuid(),
                NotificationId = notificationId,
                Channel = NotificationChannel.InApp,
                DeliveryStatus = NotificationDeliveryStatus.Pending,
                SentAt = DateTime.UtcNow
            };

            _db.NotificationHistories.Add(inAppHistory);
            await _db.SaveChangesAsync(cancellationToken);

            try
            {
                await _realtime.NotifyAsync(notification, cancellationToken);
                inAppHistory.DeliveryStatus = NotificationDeliveryStatus.Delivered;
                inAppHistory.DeliveredAt = DateTime.UtcNow;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                inAppHistory.DeliveryStatus = NotificationDeliveryStatus.Failed;
                inAppHistory.FailureReason = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                _logger.LogWarning(ex,
                    "Real-time notification {NotificationId} could not be delivered to user {UserId}.",
                    notificationId, userId);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        foreach (var channel in selectedChannels.Where(x => x != NotificationChannel.InApp))
        {
            var history = new NotificationHistory
            {
                HistoryId = Guid.NewGuid(),
                NotificationId = notificationId,
                Channel = channel,
                DeliveryStatus = NotificationDeliveryStatus.Pending,
                SentAt = DateTime.UtcNow
            };

            _db.NotificationHistories.Add(history);
            await _db.SaveChangesAsync(cancellationToken);

            try
            {
                await SendWithRetryAsync(channel, user, title, message, cancellationToken);
                history.DeliveryStatus = NotificationDeliveryStatus.Delivered;
                history.DeliveredAt = DateTime.UtcNow;
                history.FailureReason = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                history.DeliveryStatus = NotificationDeliveryStatus.Failed;
                history.FailureReason = ex.Message.Length > 1000 ? ex.Message[..1000] : ex.Message;
                _logger.LogError(ex, "Notification {NotificationId} failed on {Channel}.", notificationId, channel);
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        return notificationId;
    }

    private async Task SendWithRetryAsync(
        NotificationChannel channel,
        User user,
        string title,
        string message,
        CancellationToken cancellationToken)
    {
        var attempts = Math.Max(1, _settings.MaxAttempts);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                switch (channel)
                {
                    case NotificationChannel.Email:
                        if (string.IsNullOrWhiteSpace(user.Email))
                            throw new InvalidOperationException("User does not have an email address.");
                        await _email.SendAsync(user.Email, title, message, cancellationToken);
                        return;

                    case NotificationChannel.SMS:
                        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                            throw new InvalidOperationException("User does not have a phone number.");
                        await _sms.SendAsync(user.PhoneNumber, message, cancellationToken);
                        return;

                    default:
                        return;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastException = ex;
                if (attempt == attempts)
                    break;

                var delay = TimeSpan.FromSeconds(Math.Max(1, _settings.RetryDelaySeconds) * attempt);
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("Notification delivery failed.");
    }
}
