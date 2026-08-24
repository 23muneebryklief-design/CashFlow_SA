namespace CashFlowSA.Application.Common.Settings
{
    public sealed class RabbitMqSettings
    {
        public string HostName { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string VirtualHost { get; set; } = "/";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string QueueName { get; set; } = "cashflow.invoice.ocr";
        public string ConnectionName { get; set; } = "CashFlowSA-API";
        public bool AutomaticRecoveryEnabled { get; set; } = true;
        public int NetworkRecoverySeconds { get; set; } = 5;
        public ushort PrefetchCount { get; set; } = 1;
        public int MaxProcessingAttempts { get; set; } = 3;
    }
}
