using System.Text.Json;
using System.Text.Json.Serialization;
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
        services.AddHealthChecks();

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
                policy
                    .WithOrigins(
                        configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["*"])
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        return services;
    }
}