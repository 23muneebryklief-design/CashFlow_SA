using System.Net.Http.Headers;
using System.Text;
using CashFlowSA.Application.Common.Notifications;
using CashFlowSA.Application.Common.Settings;
using Microsoft.Extensions.Options;

namespace CashFlowSA.API.Services;

public sealed class TwilioSmsNotificationSender : ISmsNotificationSender
{
    private readonly HttpClient _httpClient;
    private readonly NotificationDeliverySettings _settings;
    private readonly ILogger<TwilioSmsNotificationSender> _logger;

    public TwilioSmsNotificationSender(
        HttpClient httpClient,
        IOptions<NotificationDeliverySettings> options,
        ILogger<TwilioSmsNotificationSender> logger)
    {
        _httpClient = httpClient;
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(string recipient, string message, CancellationToken cancellationToken = default)
    {
        var settings = _settings.Sms;
        if (!settings.Enabled)
            throw new InvalidOperationException("SMS notification delivery is disabled.");
        if (!string.Equals(settings.Provider, "Twilio", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Unsupported SMS provider '{settings.Provider}'.");
        if (string.IsNullOrWhiteSpace(settings.AccountSid) ||
            string.IsNullOrWhiteSpace(settings.AuthToken) ||
            string.IsNullOrWhiteSpace(settings.FromNumber))
            throw new InvalidOperationException("Twilio SMS settings are incomplete.");

        var endpoint = $"https://api.twilio.com/2010-04-01/Accounts/{Uri.EscapeDataString(settings.AccountSid)}/Messages.json";
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{settings.AccountSid}:{settings.AuthToken}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["To"] = recipient,
            ["From"] = settings.FromNumber,
            ["Body"] = message
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Twilio returned {(int)response.StatusCode}: {body}");
        }

        _logger.LogInformation("SMS notification sent to {Recipient}.", recipient);
    }
}
