using System.Net;
using System.Net.Http.Json;
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
using Testcontainers.PostgreSql;
using Xunit;

namespace FinanceiroApi.E2E.Tests.Scenarios;

public class FinancialFlowScenarioTests : IAsyncLifetime
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
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
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

        using var scope = _factory.Services.CreateScope();
        var dbCtx = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbCtx.Database.EnsureDeletedAsync();
        await dbCtx.Database.EnsureCreatedAsync();
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync() { await _factory.DisposeAsync(); await _postgres.DisposeAsync(); }

    [Fact]
    public async Task BankAccountTransferFlow_ShouldSucceed()
    {
        // Criar conta origem
        var createSource = new CreateBankAccountRequest("BB", "001", "0001", "11111-1", BankAccountType.Checking, 5000m, null, null);
        var sourceResp = await _client.PostAsJsonAsync("/api/v1/bank-accounts", createSource, _json);
        sourceResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var source = await sourceResp.Content.ReadFromJsonAsync<BankAccountResponse>(_json);

        // Criar conta destino
        var createDest = new CreateBankAccountRequest("Itau", "341", "0002", "22222-2", BankAccountType.Checking, 0m, null, null);
        var destResp = await _client.PostAsJsonAsync("/api/v1/bank-accounts", createDest, _json);
        destResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var dest = await destResp.Content.ReadFromJsonAsync<BankAccountResponse>(_json);

        // Transferir
        var transfer = new TransferBetweenAccountsRequest(source!.Id, dest!.Id, 1000m, "pagamento");
        var transferResp = await _client.PostAsJsonAsync("/api/v1/bank-accounts/transfer", transfer, _json);
        transferResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar saldos
        var updatedSource = await _client.GetFromJsonAsync<BankAccountResponse>($"/api/v1/bank-accounts/{source.Id}", _json);
        updatedSource!.Balance.Should().Be(4000m);

        var updatedDest = await _client.GetFromJsonAsync<BankAccountResponse>($"/api/v1/bank-accounts/{dest.Id}", _json);
        updatedDest!.Balance.Should().Be(1000m);
    }

    [Fact]
    public async Task AccountReceivableFlow_ShouldSucceed()
    {
        // Criar cliente
        var createCustomer = new CreateCustomerRequest("Cliente Teste", "12345678000195",
            PersonType.Company, $"cliente.{Guid.NewGuid():N}@test.com", null, null, 10000m);
        var customerResp = await _client.PostAsJsonAsync("/api/v1/customers", createCustomer, _json);
        customerResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var customer = await customerResp.Content.ReadFromJsonAsync<CustomerResponse>(_json);

        // Criar conta bancária
        var createBank = new CreateBankAccountRequest("BB", "001", "0001", "33333-3", BankAccountType.Checking, 0m, null, null);
        var bankResp = await _client.PostAsJsonAsync("/api/v1/bank-accounts", createBank, _json);
        var bank = await bankResp.Content.ReadFromJsonAsync<BankAccountResponse>(_json);

        // Criar conta a receber
        var createAR = new CreateAccountReceivableRequest(
            customer!.Id, "Venda de produto", 2000m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), null, "NF-001", null);
        var arResp = await _client.PostAsJsonAsync("/api/v1/accounts-receivable", createAR, _json);
        arResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var ar = await arResp.Content.ReadFromJsonAsync<AccountReceivableResponse>(_json);
        ar!.Status.Should().Be("Pending");

        // Registrar recebimento
        var receive = new ReceivePaymentRequest(2000m, DateOnly.FromDateTime(DateTime.UtcNow), bank!.Id);
        var receiveResp = await _client.PostAsJsonAsync($"/api/v1/accounts-receivable/{ar.Id}/receive", receive, _json);
        receiveResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var received = await receiveResp.Content.ReadFromJsonAsync<AccountReceivableResponse>(_json);
        received!.Status.Should().Be("Received");
    }

    [Fact]
    public async Task BudgetApprovalFlow_ShouldSucceed()
    {
        var createCC = new CreateCostCenterRequest("TI-001", "Tecnologia", 100000m, null, null, null);
        var ccResp = await _client.PostAsJsonAsync("/api/v1/cost-centers", createCC, _json);
        ccResp.StatusCode.Should().Be(HttpStatusCode.Created, "criação do centro de custos deve retornar 201");
        var cc = await ccResp.Content.ReadFromJsonAsync<CostCenterResponse>(_json);
        cc.Should().NotBeNull();
        cc!.Id.Should().NotBeEmpty();

        var createBudget = new CreateBudgetRequest(2026, "Orçamento TI 2026", "Teste E2E");
        var budgetResp = await _client.PostAsJsonAsync("/api/v1/budgets", createBudget, _json);
        budgetResp.StatusCode.Should().Be(HttpStatusCode.Created, "criação do orçamento deve retornar 201");
        var budget = await budgetResp.Content.ReadFromJsonAsync<BudgetResponse>(_json);
        budget.Should().NotBeNull();
        budget!.Id.Should().NotBeEmpty();

        var addItem = new AddBudgetItemRequest(cc.Id, "Infraestrutura", 45000m);
        var itemResp = await _client.PostAsJsonAsync($"/api/v1/budgets/{budget.Id}/items", addItem, _json);
        itemResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var itemBody = await itemResp.Content.ReadAsStringAsync();

        var itemContent = await itemResp.Content.ReadFromJsonAsync<BudgetResponse>(_json);
        itemContent.Should().NotBeNull();
        itemContent!.Items.Should().HaveCount(1,
            "o handler deve retornar o orçamento com o item recém-adicionado");

        var budgetAfterAdd = await _client.GetFromJsonAsync<BudgetResponse>(
            $"/api/v1/budgets/{budget.Id}", _json);
        budgetAfterAdd.Should().NotBeNull();
        budgetAfterAdd!.Items.Should().NotBeEmpty("o item deve estar persistido no banco");

        var approveResp = await _client.PostAsJsonAsync(
            $"/api/v1/budgets/{budget.Id}/approve",
            new ApproveBudgetRequest(Guid.NewGuid()), _json);
        approveResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SupplierBlockFlow_ShouldSucceed()
    {
        var create = new CreateSupplierRequest("Fornecedor Fluxo", "98765432000196",
            PersonType.Company, $"fornec.{Guid.NewGuid():N}@test.com", null, null);
        var createResp = await _client.PostAsJsonAsync("/api/v1/suppliers", create, _json);
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var supplier = await createResp.Content.ReadFromJsonAsync<SupplierResponse>(_json);
        supplier!.Status.Should().Be("Active");

        var blockResp = await _client.PostAsJsonAsync(
            $"/api/v1/suppliers/{supplier.Id}/block",
            new { Reason = "fraude detectada" }, _json);
        blockResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var blocked = await blockResp.Content.ReadFromJsonAsync<SupplierResponse>(_json);
        blocked!.Status.Should().Be("Blocked");
    }
}
