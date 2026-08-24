using System.Net;
using System.Net.Mail;
using CashFlowSA.Application.Common.Notifications;
using CashFlowSA.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace CashFlowSA.API.Services;

public sealed class EmailNotificationSender : IEmailNotificationSender
{
    private readonly NotificationDeliverySettings _settings;
    private readonly ILogger<EmailNotificationSender> _logger;

    public EmailNotificationSender(
        IOptions<NotificationDeliverySettings> options,
        ILogger<EmailNotificationSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string recipient, string subject, string message, CancellationToken cancellationToken = default)
    {
        var settings = _settings.Email;
        if (!settings.Enabled)
            throw new InvalidOperationException("Email notification delivery is disabled.");
        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromAddress))
            throw new InvalidOperationException("Email notification settings are incomplete.");

        using var mail = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName),
            Subject = subject,
            Body = message,
            IsBodyHtml = false
        };
        mail.To.Add(new MailAddress(recipient));

        using var client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(settings.Username, settings.Password)
        };

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(mail, cancellationToken);
        _logger.LogInformation("Email notification sent to {Recipient}.", recipient);
    }
}
