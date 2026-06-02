using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Integration.Tests.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FinanceiroApi.Integration.Tests.Fixtures;

public sealed class IntegrationTestFixture : IDisposable
{
    public HttpClient Client { get; }
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTestFixture()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                });
                builder.ConfigureServices(services =>
                {
                    var redis = services.SingleOrDefault(
                        d => d.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
                    if (redis is not null) services.Remove(redis);

                    var cache = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICacheService));
                    if (cache is not null) services.Remove(cache);

                    services.AddScoped<ICacheService, NullCacheService>();

                    var hostedServices = services
                        .Where(d => d.ImplementationType?.Namespace?.Contains("FinanceiroApi.Infrastructure.Messaging") == true)
                        .ToList();
                    hostedServices.ForEach(d => services.Remove(d));

                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                    services.PostConfigure<AuthenticationOptions>(opts =>
                    {
                        opts.DefaultAuthenticateScheme = "Test";
                        opts.DefaultChallengeScheme = "Test";
                        opts.DefaultScheme = "Test";
                    });
                });
            });

        Client = _factory.CreateClient();
    }

    public void Dispose()
    {
        Client.Dispose();
        _factory.Dispose();
    }
}