using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.BankReconciliations.GetBankReconciliationById;
using FinanceiroApi.Application.Queries.BankReconciliations.GetBankReconciliationsByAccount;
using FinanceiroApi.Application.Queries.BankStatements.GetBankStatementById;
using FinanceiroApi.Application.Queries.BankStatements.GetBankStatementsByAccount;
using FinanceiroApi.Application.Queries.Budgets.GetBudgets;
using FinanceiroApi.Application.Queries.ChartOfAccounts.GetAllChartOfAccounts;
using FinanceiroApi.Application.Queries.ChartOfAccounts.GetChartOfAccountById;
using FinanceiroApi.Application.Queries.Customers.GetAllCustomers;
using FinanceiroApi.Application.Queries.Customers.GetCustomerById;
using FinanceiroApi.Application.Queries.Departments.GetAllDepartments;
using FinanceiroApi.Application.Queries.Departments.GetDepartmentById;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Queries;

public class GetBankReconciliationByIdQueryHandlerTests
{
    private readonly IBankReconciliationRepository _repo = Substitute.For<IBankReconciliationRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();
    private GetBankReconciliationByIdQueryHandler CreateHandler() => new(_repo, _mapper, _notif);

    [Fact]
    public async Task Handle_WithExistingReconciliation_ShouldReturnResponse()
    {
        var bankAccountId  = Guid.NewGuid();
        var statementId    = Guid.NewGuid();
        var reconciliation = BankReconciliation.Create(bankAccountId, statementId,
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), 1000m, 1500m, 1400m);
        var expected = new BankReconciliationResponse(reconciliation.Id, bankAccountId, "Banco",
            statementId, new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31),
            1000m, 1500m, 1400m, 100m, false, "Open", 0, 0, 0, null, null, null, DateTime.UtcNow, null, []);
        _repo.GetWithItemsAsync(reconciliation.Id, default).Returns(reconciliation);
        _mapper.Map<BankReconciliationResponse>(reconciliation).Returns(expected);
        var result = await CreateHandler().Handle(new GetBankReconciliationByIdQuery(reconciliation.Id), default);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentReconciliation_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithItemsAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetBankReconciliationByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class GetBankReconciliationsByAccountQueryHandlerTests
{
    private readonly IBankReconciliationRepository _repo = Substitute.For<IBankReconciliationRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetBankReconciliationsByAccountQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithBankAccountId_ShouldReturnReconciliationsByAccount()
    {
        var bankAccountId = Guid.NewGuid();
        _repo.GetByBankAccountAsync(bankAccountId, default).Returns(new List<BankReconciliation>().AsReadOnly());
        _mapper.Map<IReadOnlyList<BankReconciliationSummaryResponse>>(Arg.Any<object>()).Returns(new List<BankReconciliationSummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetBankReconciliationsByAccountQuery(bankAccountId, null), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetByBankAccountAsync(bankAccountId, default);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnReconciliationsByStatus()
    {
        _repo.GetByStatusAsync(ReconciliationStatus.Completed, default).Returns(new List<BankReconciliation>().AsReadOnly());
        _mapper.Map<IReadOnlyList<BankReconciliationSummaryResponse>>(Arg.Any<object>()).Returns(new List<BankReconciliationSummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetBankReconciliationsByAccountQuery(null, ReconciliationStatus.Completed), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetByStatusAsync(ReconciliationStatus.Completed, default);
    }
}

public class GetBankStatementByIdQueryHandlerTests
{
    private readonly IBankStatementRepository _repo = Substitute.For<IBankStatementRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();
    private GetBankStatementByIdQueryHandler CreateHandler() => new(_repo, _mapper, _notif);

    [Fact]
    public async Task Handle_WithExistingStatement_ShouldReturnResponse()
    {
        var bankAccountId = Guid.NewGuid();
        var statement     = BankStatement.Create(bankAccountId, new DateOnly(2025, 1, 31),
            new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31), 1000m, 1500m, null, null);
        var expected = new BankStatementResponse(statement.Id, bankAccountId, "Banco",
            new DateOnly(2025, 1, 31), new DateOnly(2025, 1, 1), new DateOnly(2025, 1, 31),
            1000m, 1500m, "BRL", "Imported", 0, 0m, 0m, null, null, DateTime.UtcNow, null, []);
        _repo.GetWithEntriesAsync(statement.Id, default).Returns(statement);
        _mapper.Map<BankStatementResponse>(statement).Returns(expected);
        var result = await CreateHandler().Handle(new GetBankStatementByIdQuery(statement.Id), default);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentStatement_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithEntriesAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetBankStatementByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class GetBankStatementsByAccountQueryHandlerTests
{
    private readonly IBankStatementRepository _repo = Substitute.For<IBankStatementRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetBankStatementsByAccountQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithoutDateFilter_ShouldReturnAllByAccount()
    {
        var bankAccountId = Guid.NewGuid();
        _repo.GetByBankAccountAsync(bankAccountId, default).Returns(new List<BankStatement>().AsReadOnly());
        _mapper.Map<IReadOnlyList<BankStatementSummaryResponse>>(Arg.Any<object>()).Returns(new List<BankStatementSummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetBankStatementsByAccountQuery(bankAccountId, null, null), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetByBankAccountAsync(bankAccountId, default);
    }

    [Fact]
    public async Task Handle_WithDateFilter_ShouldReturnStatementsByPeriod()
    {
        var bankAccountId = Guid.NewGuid();
        var from = new DateOnly(2025, 1, 1);
        var to   = new DateOnly(2025, 1, 31);
        _repo.GetByPeriodAsync(bankAccountId, from, to, default).Returns(new List<BankStatement>().AsReadOnly());
        _mapper.Map<IReadOnlyList<BankStatementSummaryResponse>>(Arg.Any<object>()).Returns(new List<BankStatementSummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetBankStatementsByAccountQuery(bankAccountId, from, to), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetByPeriodAsync(bankAccountId, from, to, default);
    }
}

public class GetBudgetsQueryHandlerTests
{
    private readonly IBudgetRepository _repo = Substitute.For<IBudgetRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetBudgetsQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnAllBudgets()
    {
        _repo.GetAllAsync(default).Returns(new List<Budget>().AsReadOnly());
        _mapper.Map<IReadOnlyList<BudgetSummaryResponse>>(Arg.Any<object>()).Returns(new List<BudgetSummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetBudgetsQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetAllAsync(default);
    }

    [Fact]
    public async Task Handle_WithYearFilter_ShouldReturnBudgetsByYear()
    {
        _repo.GetByYearAsync(2025, default).Returns(new List<Budget>());
        _mapper.Map<IReadOnlyList<BudgetSummaryResponse>>(Arg.Any<object>()).Returns(new List<BudgetSummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetBudgetsQuery(2025), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetByYearAsync(2025, default);
    }
}

public class GetChartOfAccountByIdQueryHandlerTests
{
    private readonly IChartOfAccountRepository _repo = Substitute.For<IChartOfAccountRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetChartOfAccountByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingAccount_ShouldReturnResponse()
    {
        var account  = ChartOfAccount.Create("1.1.1", "Caixa", null, AccountType.Asset, AccountNature.Debit, true);
        var expected = new ChartOfAccountResponse(account.Id, "1.1.1", "Caixa", null, AccountType.Asset, "Asset", AccountNature.Debit, "Debit", true, true, null, null, null, null, DateTime.UtcNow, null);
        _repo.GetByIdAsync(account.Id, default).Returns(account);
        _mapper.Map<ChartOfAccountResponse>(account).Returns(expected);
        var result = await CreateHandler().Handle(new GetChartOfAccountByIdQuery(account.Id), default);
        result.Should().NotBeNull();
        result.Code.Should().Be("1.1.1");
    }

    [Fact]
    public async Task Handle_WithNonExistentAccount_ShouldThrowDomainException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var act = () => CreateHandler().Handle(new GetChartOfAccountByIdQuery(Guid.NewGuid()), default);
        await act.Should().ThrowAsync<Exception>();
    }
}

public class GetAllChartOfAccountsQueryHandlerTests
{
    private readonly IChartOfAccountRepository _repo = Substitute.For<IChartOfAccountRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAllChartOfAccountsQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnActiveAccounts()
    {
        _repo.GetActiveAccountsAsync(default).Returns(new List<ChartOfAccount>());
        _mapper.Map<IEnumerable<ChartOfAccountSummaryResponse>>(Arg.Any<object>()).Returns(new List<ChartOfAccountSummaryResponse>());
        var result = await CreateHandler().Handle(new GetAllChartOfAccountsQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetActiveAccountsAsync(default);
    }

    [Fact]
    public async Task Handle_WithOnlyRootsFlag_ShouldReturnRootAccounts()
    {
        _repo.GetRootAccountsAsync(default).Returns(new List<ChartOfAccount>());
        _mapper.Map<IEnumerable<ChartOfAccountSummaryResponse>>(Arg.Any<object>()).Returns(new List<ChartOfAccountSummaryResponse>());
        var result = await CreateHandler().Handle(new GetAllChartOfAccountsQuery(OnlyRoots: true), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetRootAccountsAsync(default);
    }
}

public class GetAllCustomersQueryHandlerTests
{
    private readonly ICustomerRepository _repo = Substitute.For<ICustomerRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAllCustomersQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnAllCustomers()
    {
        _repo.GetActiveAsync(default).Returns(new List<Customer>().AsReadOnly());
        _mapper.Map<IReadOnlyList<CustomerSummaryResponse>>(Arg.Any<object>()).Returns(new List<CustomerSummaryResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAllCustomersQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetActiveAsync(default);
    }
}

public class GetCustomerByIdQueryHandlerTests
{
    private readonly ICustomerRepository _repo = Substitute.For<ICustomerRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetCustomerByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingCustomer_ShouldReturnResponse()
    {
        var customer = Customer.Create("Cliente Ltda", "12345678000195", PersonType.Company, "cliente@email.com");
        var expected = new CustomerResponse(customer.Id, "Cliente Ltda", "12345678000195",
            "Company", "cliente@email.com", null, null, "Active", 0m, "BRL", DateTime.UtcNow, null);
        _repo.GetByIdAsync(customer.Id, default).Returns(customer);
        _mapper.Map<CustomerResponse>(customer).Returns(expected);
        var result = await CreateHandler().Handle(new GetCustomerByIdQuery(customer.Id), default);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Cliente Ltda");
    }

    [Fact]
    public async Task Handle_WithNonExistentCustomer_ShouldReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetCustomerByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
    }
}

public class GetAllDepartmentsQueryHandlerTests
{
    private readonly IDepartmentRepository _repo = Substitute.For<IDepartmentRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAllDepartmentsQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnAllDepartments()
    {
        _repo.GetActiveAsync(default).Returns(new List<Department>().AsReadOnly());
        _mapper.Map<IReadOnlyList<DepartmentResponse>>(Arg.Any<object>()).Returns(new List<DepartmentResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAllDepartmentsQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetActiveAsync(default);
    }
}

public class GetDepartmentByIdQueryHandlerTests
{
    private readonly IDepartmentRepository _repo = Substitute.For<IDepartmentRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetDepartmentByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingDepartment_ShouldReturnResponse()
    {
        var department = Department.Create("Tecnologia", "TI-001", "Depto de TI");
        var expected   = new DepartmentResponse(department.Id, "Tecnologia", "Depto de TI", "TI-001", true, 0);
        _repo.GetByIdAsync(department.Id, default).Returns(department);
        _mapper.Map<DepartmentResponse>(department).Returns(expected);
        var result = await CreateHandler().Handle(new GetDepartmentByIdQuery(department.Id), default);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Tecnologia");
    }

    [Fact]
    public async Task Handle_WithNonExistentDepartment_ShouldReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetDepartmentByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
    }
}





