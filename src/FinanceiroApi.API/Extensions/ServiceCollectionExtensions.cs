using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using FinanceiroApi.API.Filters;

namespace FinanceiroApi.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddControllers(options => options.Filters.Add<NotificationFilter>())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("Default") ?? "",
                name: "postgresql",
                tags: ["db", "sql", "postgres"]);

        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddHealthChecks()
                .AddRedis(redisConn, name: "redis", tags: ["cache"]);
        }

        var rabbitHost = configuration["RabbitMq:Host"];
        if (!string.IsNullOrWhiteSpace(rabbitHost))
        {
            var user = configuration["RabbitMq:Username"] ?? "guest";
            var pass = configuration["RabbitMq:Password"] ?? "guest";
            var port = configuration["RabbitMq:Port"] ?? "5672";
            services.AddHealthChecks()
                .AddRabbitMQ(
                    rabbitConnectionString: $"amqp://{user}:{pass}@{rabbitHost}:{port}/",
                    name: "rabbitmq",
                    tags: ["messaging"]);
        }

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
                policy
                    .WithOrigins(
                        configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["*"])
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter("auth", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 10;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit = 0;
            });

            options.AddFixedWindowLimiter("general", o =>
            {
                o.Window = TimeSpan.FromMinutes(1);
                o.PermitLimit = 100;
                o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                o.QueueLimit = 5;
            });
        });

        return services;
    }
}