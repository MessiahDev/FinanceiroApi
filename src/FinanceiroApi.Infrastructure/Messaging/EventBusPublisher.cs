using System.Text;
using System.Text.Json;
using FinanceiroApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FinanceiroApi.Infrastructure.Messaging;

public sealed class EventBusPublisher : IEventBusPublisher, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly ILogger<EventBusPublisher> _logger;
    private readonly ConnectionFactory _factory;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public EventBusPublisher(
        IOptions<RabbitMqSettings> options,
        ILogger<EventBusPublisher> logger)
    {
        _logger = logger;
        var cfg = options.Value;

        _factory = new ConnectionFactory
        {
            HostName = cfg.Host,
            Port = cfg.Port,
            VirtualHost = cfg.VirtualHost,
            UserName = cfg.Username,
            Password = cfg.Password,
        };
    }

    private async Task EnsureChannelCreatedAsync(CancellationToken cancellationToken = default)
    {
        if (_channel is not null) return;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_channel is null)
            {
                _connection = await _factory.CreateConnectionAsync(cancellationToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class
    {
        await EnsureChannelCreatedAsync(cancellationToken);

        var exchangeName = typeof(T).Name.ToLowerInvariant();

        await _channel!.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        var props = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await _channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: string.Empty,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Event published to exchange '{Exchange}': {EventType}",
            exchangeName, typeof(T).Name);
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
            await _channel.CloseAsync();

        if (_connection is not null)
            await _connection.CloseAsync();

        _lock.Dispose();
    }
}
