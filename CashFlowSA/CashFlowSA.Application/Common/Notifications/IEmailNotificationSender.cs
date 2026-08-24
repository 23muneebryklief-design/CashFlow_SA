namespace CashFlowSA.Application.Common.Notifications;

public interface IEmailNotificationSender
{
    Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default);
}
