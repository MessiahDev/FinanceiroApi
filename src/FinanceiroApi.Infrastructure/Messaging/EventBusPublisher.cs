using System.Text;
using System.Text.Json;
using FinanceiroApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FinanceiroApi.Infrastructure.Messaging;

public sealed class EventBusPublisher : IEventBusPublisher, IDisposable
{
    private IConnection? _connection;
    private IModel? _channel;
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
            DispatchConsumersAsync = true
        };
    }

    private void EnsureChannelCreated()
    {
        if (_channel is not null) return;

        _lock.Wait();
        try
        {
            if (_channel is null)
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default)
        where T : class
    {
        EnsureChannelCreated();

        var exchangeName = typeof(T).Name.ToLowerInvariant();

        _channel!.ExchangeDeclare(
            exchange: exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false);

        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        if (_channel is null) return Task.CompletedTask;
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.MessageId = Guid.NewGuid().ToString();
        props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        _channel?.BasicPublish(
            exchange: exchangeName,
            routingKey: string.Empty,
            basicProperties: props,
            body: body);

        _logger.LogInformation(
            "Event published to exchange '{Exchange}': {Event}",
            exchangeName, typeof(T).Name);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _lock.Dispose();
    }
}




