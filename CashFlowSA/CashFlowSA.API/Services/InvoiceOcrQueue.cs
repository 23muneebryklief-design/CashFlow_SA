using System.Runtime.CompilerServices;
using System.Text.Json;
using CashFlowSA.Application.Common.Ocr;
using CashFlowSA.Application.Common.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CashFlowSA.API.Services
{
    public sealed class InvoiceOcrQueue : IInvoiceOcrQueue, IAsyncDisposable
    {
        private readonly RabbitMqSettings _settings;
        private readonly ILogger<InvoiceOcrQueue> _logger;
        private readonly SemaphoreSlim _connectionLock = new(1, 1);
        private readonly SemaphoreSlim _publishLock = new(1, 1);
        private readonly System.Threading.Channels.Channel<InvoiceOcrMessage> _messages =
            System.Threading.Channels.Channel.CreateUnbounded<InvoiceOcrMessage>(
                new System.Threading.Channels.UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = false
                });

        private IConnection? _connection;
        private IChannel? _publisherChannel;
        private IChannel? _consumerChannel;
        private bool _consumerStarted;

        public InvoiceOcrQueue(IOptions<RabbitMqSettings> options, ILogger<InvoiceOcrQueue> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async ValueTask EnqueueAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        {
            await EnsureConnectionAsync(cancellationToken);
            await EnsurePublisherChannelAsync(cancellationToken);

            var payload = JsonSerializer.SerializeToUtf8Bytes(new InvoiceOcrMessagePayload(invoiceId));
            var properties = new BasicProperties
            {
                ContentType = "application/json",
                DeliveryMode = DeliveryModes.Persistent,
                MessageId = invoiceId.ToString()
            };

            await _publishLock.WaitAsync(cancellationToken);
            try
            {
                await _publisherChannel!.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: _settings.QueueName,
                    mandatory: true,
                    basicProperties: properties,
                    body: payload,
                    cancellationToken: cancellationToken);
            }
            finally
            {
                _publishLock.Release();
            }
        }

        public async IAsyncEnumerable<InvoiceOcrMessage> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await EnsureConsumerAsync(cancellationToken);

            await foreach (var message in _messages.Reader.ReadAllAsync(cancellationToken))
                yield return message;
        }

        private async Task EnsureConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection is { IsOpen: true })
                return;

            await _connectionLock.WaitAsync(cancellationToken);
            try
            {
                if (_connection is { IsOpen: true })
                    return;

                await DisposeChannelsAsync();

                var factory = new ConnectionFactory
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    VirtualHost = _settings.VirtualHost,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    ClientProvidedName = _settings.ConnectionName,
                    AutomaticRecoveryEnabled = _settings.AutomaticRecoveryEnabled,
                    TopologyRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(Math.Max(1, _settings.NetworkRecoverySeconds)),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(15)
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);

                _logger.LogInformation(
                    "Connected to RabbitMQ at {Host}:{Port}; queue {Queue}.",
                    _settings.HostName, _settings.Port, _settings.QueueName);
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        private async Task EnsurePublisherChannelAsync(CancellationToken cancellationToken)
        {
            if (_publisherChannel is { IsOpen: true })
                return;

            await EnsureConnectionAsync(cancellationToken);

            if (_publisherChannel is { IsOpen: true })
                return;

            _publisherChannel = await _connection!.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: true,
                    publisherConfirmationTrackingEnabled: true),
                cancellationToken);

            await DeclareQueueAsync(_publisherChannel, cancellationToken);
        }

        private async Task EnsureConsumerAsync(CancellationToken cancellationToken)
        {
            if (_consumerStarted && _consumerChannel is { IsOpen: true })
                return;

            await EnsureConnectionAsync(cancellationToken);

            _consumerChannel = await _connection!.CreateChannelAsync(
                new CreateChannelOptions(
                    publisherConfirmationsEnabled: false,
                    publisherConfirmationTrackingEnabled: false,
                    consumerDispatchConcurrency: 1),
                cancellationToken);

            await DeclareQueueAsync(_consumerChannel, cancellationToken);

            await _consumerChannel.BasicQosAsync(
                prefetchSize: 0,
                prefetchCount: _settings.PrefetchCount,
                global: false,
                cancellationToken: cancellationToken);

            var channel = _consumerChannel;
            var consumer = new RabbitMQ.Client.Events.AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (_, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();

                InvoiceOcrMessagePayload? payload;
                try
                {
                    payload = JsonSerializer.Deserialize<InvoiceOcrMessagePayload>(body);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Invalid invoice OCR message received.");
                    await channel.BasicRejectAsync(eventArgs.DeliveryTag, requeue: false);
                    return;
                }

                if (payload is null || payload.InvoiceId == Guid.Empty)
                {
                    _logger.LogError("Invalid invoice OCR message received.");
                    await channel.BasicRejectAsync(eventArgs.DeliveryTag, requeue: false);
                    return;
                }

                var deliveryTag = eventArgs.DeliveryTag;

                await _messages.Writer.WriteAsync(
                    new InvoiceOcrMessage(
                        payload.InvoiceId,
                        ack: () => channel.BasicAckAsync(deliveryTag, multiple: false),
                        reject: () => channel.BasicRejectAsync(deliveryTag, requeue: false)),
                    cancellationToken);
            };

            await channel.BasicConsumeAsync(
                queue: _settings.QueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: cancellationToken);

            _consumerStarted = true;

            _logger.LogInformation(
                "RabbitMQ OCR consumer started for queue {Queue}.",
                _settings.QueueName);
        }

        private async Task DeclareQueueAsync(IChannel channel, CancellationToken cancellationToken)
        {
            await channel.QueueDeclareAsync(
                queue: _settings.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
        }

        private async Task DisposeChannelsAsync()
        {
            _consumerStarted = false;

            if (_consumerChannel is not null)
            {
                try { await _consumerChannel.CloseAsync(); } catch { }
                await _consumerChannel.DisposeAsync();
                _consumerChannel = null;
            }

            if (_publisherChannel is not null)
            {
                try { await _publisherChannel.CloseAsync(); } catch { }
                await _publisherChannel.DisposeAsync();
                _publisherChannel = null;
            }

            if (_connection is not null)
            {
                try { await _connection.CloseAsync(); } catch { }
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            _messages.Writer.TryComplete();

            await _connectionLock.WaitAsync();
            try
            {
                await DisposeChannelsAsync();
            }
            finally
            {
                _connectionLock.Release();
                _connectionLock.Dispose();
                _publishLock.Dispose();
            }
        }

        private sealed record InvoiceOcrMessagePayload(Guid InvoiceId);
    }
}
