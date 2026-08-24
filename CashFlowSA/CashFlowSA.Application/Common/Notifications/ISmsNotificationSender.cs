namespace CashFlowSA.Application.Common.Notifications;

public interface ISmsNotificationSender
{
    Task SendAsync(string recipient, string message, CancellationToken cancellationToken = default);
}
