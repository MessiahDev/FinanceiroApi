using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.Accounting.GetTrialBalance;
using FinanceiroApi.Application.Queries.Reports.GetFinancialSummary;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Queries;

public class GetTrialBalanceQueryHandlerTests
{
    private readonly IAccountingPeriodRepository _periodRepo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IJournalEntryRepository _entryRepo = Substitute.For<IJournalEntryRepository>();
    private readonly IChartOfAccountRepository _accountRepo = Substitute.For<IChartOfAccountRepository>();
    private GetTrialBalanceQueryHandler CreateHandler() => new(_periodRepo, _entryRepo, _accountRepo);

    [Fact]
    public async Task Handle_WithValidPeriod_ShouldReturnTrialBalance()
    {
        var period = AccountingPeriod.Create(2025, 1);
        _periodRepo.GetByIdAsync(period.Id, default).Returns(period);
        _entryRepo.GetPostedEntriesAsync(period.Id, default).Returns(new List<JournalEntry>());
        _accountRepo.GetActiveAccountsAsync(default).Returns(new List<ChartOfAccount>());
        var result = await CreateHandler().Handle(new GetTrialBalanceQuery(period.Id), default);
        result.Should().NotBeNull();
        result.AccountingPeriodId.Should().Be(period.Id);
    }

    [Fact]
    public async Task Handle_WithNonExistentPeriod_ShouldThrowDomainException()
    {
        _periodRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var act = () => CreateHandler().Handle(new GetTrialBalanceQuery(Guid.NewGuid()), default);
        await act.Should().ThrowAsync<Exception>();
    }
}

public class GetFinancialSummaryQueryHandlerTests
{
    private readonly ITransactionRepository _transactionRepo = Substitute.For<ITransactionRepository>();
    private readonly IPayrollRepository _payrollRepo = Substitute.For<IPayrollRepository>();
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private GetFinancialSummaryQueryHandler CreateHandler() =>
        new(_transactionRepo, _payrollRepo, _employeeRepo, _cache);

    [Fact]
    public async Task Handle_WithValidPeriod_ShouldReturnFinancialSummary()
    {
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 1, 31);
        _cache.GetAsync<FinancialSummaryResponse>(Arg.Any<string>(), default).ReturnsNull();
        _transactionRepo.GetByPeriodAsync(from, to, default).Returns(new List<Transaction>().AsReadOnly());
        _payrollRepo.GetProcessedByPeriodAsync(from, to, default).Returns(new List<Payroll>().AsReadOnly());
        _employeeRepo.CountActiveAsync(default).Returns(10);
        var result = await CreateHandler().Handle(new GetFinancialSummaryQuery(from, to), default);
        result.Should().NotBeNull();
        result.From.Should().Be(from);
        result.To.Should().Be(to);
    }

    [Fact]
    public async Task Handle_WhenCached_ShouldReturnCachedSummary()
    {
        var from = new DateOnly(2025, 1, 1);
        var to = new DateOnly(2025, 1, 31);
        var cached = new FinancialSummaryResponse(from, to, 0m, 0m, 0m, 0, 0m, 5, [], []);
        _cache.GetAsync<FinancialSummaryResponse>(Arg.Any<string>(), default).Returns(cached);
        var result = await CreateHandler().Handle(new GetFinancialSummaryQuery(from, to), default);
        result.Should().NotBeNull();
        result.ActiveEmployees.Should().Be(5);
        await _transactionRepo.DidNotReceive().GetByPeriodAsync(Arg.Any<DateOnly>(), Arg.Any<DateOnly>(), default);
    }
}
