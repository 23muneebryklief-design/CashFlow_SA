namespace CashFlowSA.Application.Common.Settings;

public sealed class NotificationDeliverySettings
{
    public EmailNotificationSettings Email { get; set; } = new();
    public SmsNotificationSettings Sms { get; set; } = new();
    public int MaxAttempts { get; set; } = 3;
    public int RetryDelaySeconds { get; set; } = 2;
}

public sealed class EmailNotificationSettings
{
    public bool Enabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = "CashFlowSA";
}

public sealed class SmsNotificationSettings
{
    public bool Enabled { get; set; }
    public string Provider { get; set; } = "Twilio";
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromNumber { get; set; } = string.Empty;
}
