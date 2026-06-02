namespace FinanceiroApi.Infrastructure.Messaging;

public sealed class RabbitMqSettings
{
    public const string SectionName = "RabbitMQ";
    public string Host { get; init; } = "localhost";
    public string VirtualHost { get; init; } = "/";
    public string Username { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public int Port { get; init; } = 5672;
}
