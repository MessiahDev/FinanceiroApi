using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinanceiroApi.Infrastructure.Messaging;

public abstract class RabbitMqConsumerBase : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;
    protected readonly ILogger Logger;
    private readonly RabbitMqSettings _cfg;

    protected abstract string ExchangeName { get; }
    protected abstract string QueueName { get; }

    protected RabbitMqConsumerBase(IOptions<RabbitMqSettings> options, ILogger logger)
    {
        _cfg = options.Value;
        Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _cfg.Host,
            Port = _cfg.Port,
            VirtualHost = _cfg.VirtualHost,
            UserName = _cfg.Username,
            Password = _cfg.Password,
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.ExchangeDeclareAsync(
            ExchangeName, ExchangeType.Fanout, durable: true,
            cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(
            QueueName, durable: true, exclusive: false, autoDelete: false,
            cancellationToken: stoppingToken);

        await _channel.QueueBindAsync(
            QueueName, ExchangeName, routingKey: string.Empty,
            cancellationToken: stoppingToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0, prefetchCount: 1, global: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                await HandleAsync(json, stoppingToken);
                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing message on queue '{Queue}'", QueueName);
                await _channel.BasicNackAsync(
                    ea.DeliveryTag, multiple: false, requeue: false, stoppingToken);
            }
        };

        await _channel.BasicConsumeAsync(
            QueueName, autoAck: false, consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }

    protected abstract Task HandleAsync(string json, CancellationToken cancellationToken);

    public override async void Dispose()
    {
        if (_channel is not null)
            await _channel.CloseAsync();

        if (_connection is not null)
            await _connection.CloseAsync();

        base.Dispose();
    }
}

public sealed class PayrollProcessedConsumer : RabbitMqConsumerBase
{
    public PayrollProcessedConsumer(
        IOptions<RabbitMqSettings> options,
        ILogger<PayrollProcessedConsumer> logger)
        : base(options, logger) { }

    protected override string ExchangeName => "payrollprocessed";
    protected override string QueueName => "payroll-processed-queue";

    protected override Task HandleAsync(string json, CancellationToken cancellationToken)
    {
        Logger.LogInformation("PayrollProcessed received: {Json}", json);
        return Task.CompletedTask;
    }
}

public sealed class PayrollCancelledConsumer : RabbitMqConsumerBase
{
    public PayrollCancelledConsumer(
        IOptions<RabbitMqSettings> options,
        ILogger<PayrollCancelledConsumer> logger)
        : base(options, logger) { }

    protected override string ExchangeName => "payrollcancelled";
    protected override string QueueName => "payroll-cancelled-queue";

    protected override Task HandleAsync(string json, CancellationToken cancellationToken)
    {
        Logger.LogInformation("PayrollCancelled received: {Json}", json);
        return Task.CompletedTask;
    }
}

public sealed class EmployeeCreatedConsumer : RabbitMqConsumerBase
{
    public EmployeeCreatedConsumer(
        IOptions<RabbitMqSettings> options,
        ILogger<EmployeeCreatedConsumer> logger)
        : base(options, logger) { }

    protected override string ExchangeName => "employeecreated";
    protected override string QueueName => "employee-created-queue";

    protected override Task HandleAsync(string json, CancellationToken cancellationToken)
    {
        Logger.LogInformation("EmployeeCreated received: {Json}", json);
        return Task.CompletedTask;
    }
}
