using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinanceiroApi.Infrastructure.Messaging;

public abstract class RabbitMqConsumerBase : BackgroundService
{
    private IConnection? _connection;
    private IModel? _channel;
    protected readonly ILogger Logger;
    private readonly RabbitMqSettings _cfg;

    protected abstract string ExchangeName { get; }
    protected abstract string QueueName { get; }

    protected RabbitMqConsumerBase(IOptions<RabbitMqSettings> options, ILogger logger)
    {
        _cfg = options.Value;
        Logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _cfg.Host,
            Port = _cfg.Port,
            VirtualHost = _cfg.VirtualHost,
            UserName = _cfg.Username,
            Password = _cfg.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout, durable: true);
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, routingKey: string.Empty);
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                await HandleAsync(json, stoppingToken);
                _channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing message on queue '{Queue}'", QueueName);
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(QueueName, autoAck: false, consumer: consumer);
        return Task.CompletedTask;
    }

    protected abstract Task HandleAsync(string json, CancellationToken cancellationToken);

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
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
