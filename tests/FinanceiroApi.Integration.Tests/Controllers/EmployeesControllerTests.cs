using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Xunit;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FinanceiroApi.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using FinanceiroApi.Integration.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceiroApi.Integration.Tests.Controllers;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "test@test.com"),
            new Claim(ClaimTypes.Role, "Admin"),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public sealed class EmployeesControllerTests : IAsyncLifetime
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
    public async Task GetAll_ShouldReturn200_WhenCalled()
    {
        var response = await _client.GetAsync("/api/v1/employees");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenEmployeeDoesNotExist()
    {
        var response = await _client.GetAsync($"/api/v1/employees/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ShouldReturn201_WhenRequestIsValid()
    {
        var request = new CreateEmployeeRequest(
            FirstName: "Joao",
            LastName: "Silva",
            Email: $"joao.{Guid.NewGuid():N}@teste.com",
            Cpf: "52998224725",
            Position: Position.DesenvolvedorJunior,
            DepartmentId: _departmentId,
            Salary: 5000m,
            ContractType: ContractType.CLT);

        var response = await _client.PostAsJsonAsync("/api/v1/employees", request, _jsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<EmployeeResponse>(_jsonOptions);
        created.Should().NotBeNull();
        created!.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Create_ShouldReturn400_WhenEmailIsInvalid()
    {
        var request = new CreateEmployeeRequest(
            FirstName: "Teste",
            LastName: "Invalido",
            Email: "nao-e-um-email",
            Cpf: "52998224725",
            Position: Position.DesenvolvedorJunior,
            DepartmentId: _departmentId,
            Salary: 3000m,
            ContractType: ContractType.CLT);

        var response = await _client.PostAsJsonAsync("/api/v1/employees", request, _jsonOptions);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
