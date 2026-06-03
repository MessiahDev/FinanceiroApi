using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Infrastructure.Data;
using FinanceiroApi.Integration.Tests.Fixtures;
using FinanceiroApi.Integration.Tests.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

namespace FinanceiroApi.E2E.Tests.Scenarios;

public class PayrollFlowScenarioTests : IAsyncLifetime
{
    private Guid _departmentId;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithImage("postgres:16-alpine")
        .WithDatabase("financeiro_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private WebApplicationFactory<Program> _factory = default!;
    private HttpClient _client = default!;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

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
                    var dbDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (dbDescriptor is not null) services.Remove(dbDescriptor);

                    services.AddDbContext<AppDbContext>(options =>
                        options.UseNpgsql(_postgres.GetConnectionString()));

                    var redis = services.SingleOrDefault(
                        d => d.ServiceType == typeof(StackExchange.Redis.IConnectionMultiplexer));
                    if (redis is not null) services.Remove(redis);

                    var cache = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICacheService));
                    if (cache is not null) services.Remove(cache);

                    services.AddScoped<ICacheService, NullCacheService>();

                    var emailDescriptors = services
                        .Where(d => d.ImplementationType?.Name?.Contains("SendGrid") == true)
                        .ToList();
                    emailDescriptors.ForEach(d => services.Remove(d));
                    services.AddScoped<IEmailSender, NullEmailSender>();

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

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var department = Department.Create("TI", "CC-001", "Tecnologia da Informacao");
        _departmentId = department.Id;
        db.Departments.Add(department);
        await db.SaveChangesAsync();

        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    [Fact]
    public async Task FullPayrollFlow_ShouldSucceed()
    {
        var createEmployee = new CreateEmployeeRequest(
            "Carlos",
            "Teste",
            "carlos@empresa.com",
            "52998224725",
            Position.DesenvolvedorSenior,
            _departmentId,
            8000m,
            ContractType.CLT,
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)));

        var createResp = await _client.PostAsJsonAsync("/api/v1/employees", createEmployee, _jsonOptions);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var employee = await createResp.Content.ReadFromJsonAsync<EmployeeResponse>(_jsonOptions);
        employee.Should().NotBeNull();

        var now = DateTime.UtcNow;
        var processPayroll = new ProcessPayrollRequest(now.Month, now.Year, [employee!.Id]);
        var processResp = await _client.PostAsJsonAsync("/api/v1/payroll/process", processPayroll, _jsonOptions);
        processResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var payroll = await processResp.Content.ReadFromJsonAsync<PayrollResponse>(_jsonOptions);
        payroll.Should().NotBeNull();
        payroll!.Status.Should().BeOneOf("Processed", "Processing");

        var duplicateResp = await _client.PostAsJsonAsync("/api/v1/payroll/process", processPayroll, _jsonOptions);
        duplicateResp.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);

        var cancelResp = await _client.PostAsJsonAsync($"/api/v1/payroll/{payroll.Id}/cancel", "Cancelamento para teste", _jsonOptions);
        cancelResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResp = await _client.GetAsync($"/api/v1/payroll/{payroll.Id}");
        var cancelledPayroll = await getResp.Content.ReadFromJsonAsync<PayrollDetailResponse>(_jsonOptions);
        cancelledPayroll.Should().NotBeNull();
    }

    [Fact]
    public async Task FinancialSummary_ShouldReturnAggregatedData()
    {
        var start = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
        var end = DateOnly.FromDateTime(DateTime.UtcNow);

        var resp = await _client.GetAsync($"/api/v1/reports/financial-summary?periodStart={start:yyyy-MM-dd}&periodEnd={end:yyyy-MM-dd}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await resp.Content.ReadFromJsonAsync<FinancialSummaryResponse>(_jsonOptions);
        summary.Should().NotBeNull();
        summary!.From.Should().Be(start);
    }
}

