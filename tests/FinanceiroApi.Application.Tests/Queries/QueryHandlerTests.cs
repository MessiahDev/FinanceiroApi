using AutoMapper;
using FinanceiroApi.Application.Queries.BankAccounts.GetBankAccountById;
using FinanceiroApi.Application.Queries.Budgets.GetBudgetById;
using FinanceiroApi.Application.Queries.CostCenters.GetAllCostCenters;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Queries;

public class GetBankAccountByIdQueryHandlerTests
{
    private readonly IBankAccountRepository _repo = Substitute.For<IBankAccountRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    private GetBankAccountByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingAccount_ShouldReturnResponse()
    {
        var account = BankAccount.Create("BB", "001", "1234", "56789-0", BankAccountType.Checking, 1000m);
        var expected = new BankAccountResponse(account.Id, "BB", "001", "1234", "56789-0",
            "Checking", null, 1000m, "BRL", true, null, DateTime.UtcNow, null);

        _repo.GetByIdAsync(account.Id, default).Returns(account);
        _mapper.Map<BankAccountResponse>(account).Returns(expected);

        var result = await CreateHandler().Handle(new GetBankAccountByIdQuery(account.Id), default);

        result.Should().NotBeNull();
        result!.BankName.Should().Be("BB");
    }

    [Fact]
    public async Task Handle_WithNonExistentAccount_ShouldReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetBankAccountByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
    }
}

public class GetBudgetByIdQueryHandlerTests
{
    private readonly IBudgetRepository _repo = Substitute.For<IBudgetRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();

    private GetBudgetByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingBudget_ShouldReturnResponse()
    {
        var budget = Budget.Create(2025, "Orcamento");
        var expected = new BudgetResponse(budget.Id, 2025, "Orcamento", null, "Draft",
            0m, 0m, 0m, "BRL", null, null, DateTime.UtcNow, null, []);

        _repo.GetWithItemsAsync(budget.Id, default).Returns(budget);
        _mapper.Map<BudgetResponse>(budget).Returns(expected);

        var result = await CreateHandler().Handle(new GetBudgetByIdQuery(budget.Id), default);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Orcamento");
    }

    [Fact]
    public async Task Handle_WithNonExistentBudget_ShouldReturnNull()
    {
        _repo.GetWithItemsAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetBudgetByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
    }
}

public class GetAllCostCentersQueryHandlerTests
{
    private readonly ICostCenterRepository _repo = Substitute.For<ICostCenterRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAllCostCentersQueryHandler CreateHandler() => new(_repo, _mapper);
    [Fact]
    public async Task Handle_ShouldReturnAllActiveCostCenters()
    {
        var cc1 = CostCenter.Create("TI-001", "Tecnologia", 50000m);
        var cc2 = CostCenter.Create("RH-001", "Recursos Humanos", 30000m);
        var list = new List<CostCenter> { cc1, cc2 }.AsReadOnly();
        var expected = new List<CostCenterResponse>
        {
            new(cc1.Id, "TI-001", "Tecnologia", null, null, null, 50000m, "BRL", "Active", null, null, DateTime.UtcNow, null),
            new(cc2.Id, "RH-001", "Recursos Humanos", null, null, null, 30000m, "BRL", "Active", null, null, DateTime.UtcNow, null),
        }.AsReadOnly();
        _repo.GetActivePagedAsync(1, 20, default).Returns((list, 2));
        _mapper.Map<IReadOnlyList<CostCenterResponse>>(list).Returns(expected);
        var result = await CreateHandler().Handle(new GetAllCostCentersQuery(), default);
        result.Items.Should().HaveCount(2);
    }
    [Fact]
    public async Task Handle_WhenEmpty_ShouldReturnEmptyList()
    {
        _repo.GetActivePagedAsync(1, 20, default).Returns((new List<CostCenter>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<CostCenterResponse>>(Arg.Any<object>())
               .Returns(new List<CostCenterResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAllCostCentersQuery(), default);
        result.Items.Should().BeEmpty();
    }
}
