using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Infrastructure.Data;
using FinanceiroApi.Integration.Tests.Controllers;
using FinanceiroApi.Integration.Tests.Fixtures;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using Xunit;

namespace FinanceiroApi.Integration.Tests.Controllers;

public sealed class SuppliersControllerTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithImage("postgres:16-alpine").WithDatabase("financeiro_test")
        .WithUsername("test").WithPassword("test").Build();

    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = BuildFactory();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync() { await _factory.DisposeAsync(); await _postgres.DisposeAsync(); }

    private WebApplicationFactory<Program> BuildFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(l => { l.ClearProviders(); l.AddConsole(); });
            builder.ConfigureServices(services =>
            {
                var db = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (db is not null) services.Remove(db);
                services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_postgres.GetConnectionString()));

                var redis = services.SingleOrDefault(d => d.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
                if (redis is not null) services.Remove(redis);
                var cache = services.SingleOrDefault(d => d.ServiceType == typeof(ICacheService));
                if (cache is not null) services.Remove(cache);
                services.AddScoped<ICacheService, NullCacheService>();

                var email = services.Where(d => d.ImplementationType?.Name?.Contains("SendGrid") == true).ToList();
                email.ForEach(d => services.Remove(d));
                services.AddScoped<IEmailSender, NullEmailSender>();

                var hosted = services.Where(d => d.ImplementationType?.Namespace?.Contains("FinanceiroApi.Infrastructure.Messaging") == true).ToList();
                hosted.ForEach(d => services.Remove(d));

                services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultAuthenticateScheme = "Test";
                    o.DefaultChallengeScheme    = "Test";
                    o.DefaultScheme             = "Test";
                });
            });
        });

    [Fact]
    public async Task GetAll_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/v1/suppliers");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/v1/suppliers/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201()
    {
        var request = new CreateSupplierRequest(
            "Fornecedor Teste", "12345678000195", PersonType.Company,
            $"supplier.{Guid.NewGuid():N}@test.com", null, null);

        var response = await _client.PostAsJsonAsync("/api/v1/suppliers", request, _json);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Create_WithInvalidEmail_ShouldReturn400()
    {
        var request = new CreateSupplierRequest(
            "Fornecedor", "12345678000195", PersonType.Company,
            "email-invalido", null, null);

        var response = await _client.PostAsJsonAsync("/api/v1/suppliers", request, _json);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

public sealed class BankAccountsControllerTests : IAsyncLifetime
{
    private static readonly JsonSerializerOptions _json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithImage("postgres:16-alpine").WithDatabase("financeiro_test")
        .WithUsername("test").WithPassword("test").Build();

    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        _factory = BuildFactory();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync() { await _factory.DisposeAsync(); await _postgres.DisposeAsync(); }

    private WebApplicationFactory<Program> BuildFactory() =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureLogging(l => { l.ClearProviders(); l.AddConsole(); });
            builder.ConfigureServices(services =>
            {
                var db = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (db is not null) services.Remove(db);
                services.AddDbContext<AppDbContext>(o => o.UseNpgsql(_postgres.GetConnectionString()));

                var redis = services.SingleOrDefault(d => d.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
                if (redis is not null) services.Remove(redis);
                var cache = services.SingleOrDefault(d => d.ServiceType == typeof(ICacheService));
                if (cache is not null) services.Remove(cache);
                services.AddScoped<ICacheService, NullCacheService>();

                var email = services.Where(d => d.ImplementationType?.Name?.Contains("SendGrid") == true).ToList();
                email.ForEach(d => services.Remove(d));
                services.AddScoped<IEmailSender, NullEmailSender>();

                var hosted = services.Where(d => d.ImplementationType?.Namespace?.Contains("FinanceiroApi.Infrastructure.Messaging") == true).ToList();
                hosted.ForEach(d => services.Remove(d));

                services.AddAuthentication("Test").AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });
                services.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultAuthenticateScheme = "Test";
                    o.DefaultChallengeScheme    = "Test";
                    o.DefaultScheme             = "Test";
                });
            });
        });

    [Fact]
    public async Task GetAll_ShouldReturn200()
    {
        var response = await _client.GetAsync("/api/v1/bank-accounts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_WhenNotFound_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/v1/bank-accounts/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturn201()
    {
        var request = new CreateBankAccountRequest(
            "Banco do Brasil", "001", "1234", "56789-0",
            BankAccountType.Checking, 1000m, null, null);

        var response = await _client.PostAsJsonAsync("/api/v1/bank-accounts", request, _json);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
