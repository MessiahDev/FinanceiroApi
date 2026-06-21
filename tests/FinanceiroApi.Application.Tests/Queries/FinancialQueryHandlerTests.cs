using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.AccountingPeriods.GetAccountingPeriodById;
using FinanceiroApi.Application.Queries.AccountingPeriods.GetAllAccountingPeriods;
using FinanceiroApi.Application.Queries.AccountsPayable.GetAccountPayableById;
using FinanceiroApi.Application.Queries.AccountsPayable.GetAccountsPayable;
using FinanceiroApi.Application.Queries.AccountsReceivable.GetAccountsReceivable;
using FinanceiroApi.Application.Queries.BankAccounts.GetAllBankAccounts;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Queries;

public class GetAccountingPeriodByIdQueryHandlerTests
{
    private readonly IAccountingPeriodRepository _repo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAccountingPeriodByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingPeriod_ShouldReturnResponse()
    {
        var period = AccountingPeriod.Create(2025, 1);
        var expected = new AccountingPeriodResponse(period.Id, "Jan/2025", 2025, 1,
            new DateTime(2025, 1, 1), new DateTime(2025, 1, 31), AccountingPeriodStatus.Open, "Open", 0, DateTime.UtcNow, null);
        _repo.GetByIdAsync(period.Id, default).Returns(period);
        _mapper.Map<AccountingPeriodResponse>(period).Returns(expected);
        var result = await CreateHandler().Handle(new GetAccountingPeriodByIdQuery(period.Id), default);
        result.Should().NotBeNull();
        result.Year.Should().Be(2025);
    }

    [Fact]
    public async Task Handle_WithNonExistentPeriod_ShouldThrowDomainException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var act = () => CreateHandler().Handle(new GetAccountingPeriodByIdQuery(Guid.NewGuid()), default);
        await act.Should().ThrowAsync<Exception>();
    }
}

public class GetAllAccountingPeriodsQueryHandlerTests
{
    private readonly IAccountingPeriodRepository _repo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAllAccountingPeriodsQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithoutYear_ShouldReturnOpenPeriods()
    {
        _repo.GetPagedAsync(null, 1, 20, default).Returns((new List<AccountingPeriod>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<AccountingPeriodResponse>>(Arg.Any<object>()).Returns(new List<AccountingPeriodResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAllAccountingPeriodsQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(null, 1, 20, default);
    }

    [Fact]
    public async Task Handle_WithYear_ShouldReturnPeriodsByYear()
    {
        _repo.GetPagedAsync(2025, 1, 20, default).Returns((new List<AccountingPeriod>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<AccountingPeriodResponse>>(Arg.Any<object>()).Returns(new List<AccountingPeriodResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAllAccountingPeriodsQuery(2025), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(2025, 1, 20, default);
    }
}

public class GetAccountPayableByIdQueryHandlerTests
{
    private readonly IAccountPayableRepository _repo = Substitute.For<IAccountPayableRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAccountPayableByIdQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithExistingPayable_ShouldReturnResponse()
    {
        var payable = AccountPayable.Create(Guid.NewGuid(), "Fatura 001", 1000m, DateOnly.FromDateTime(DateTime.Today.AddDays(30)));
        var expected = new AccountPayableResponse(payable.Id, payable.SupplierId, "Fornecedor", null, null, "Fatura 001", 1000m, 0m, 1000m, "BRL", payable.DueDate, null, "Pending", null, null, DateTime.UtcNow, null);
        _repo.GetWithDetailsAsync(payable.Id, default).Returns(payable);
        _mapper.Map<AccountPayableResponse>(payable).Returns(expected);
        var result = await CreateHandler().Handle(new GetAccountPayableByIdQuery(payable.Id), default);
        result.Should().NotBeNull();
        result!.Description.Should().Be("Fatura 001");
    }

    [Fact]
    public async Task Handle_WithNonExistentPayable_ShouldReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var result = await CreateHandler().Handle(new GetAccountPayableByIdQuery(Guid.NewGuid()), default);
        result.Should().BeNull();
    }
}

public class GetAccountsPayableQueryHandlerTests
{
    private readonly IAccountPayableRepository _repo = Substitute.For<IAccountPayableRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAccountsPayableQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnAllPayables()
    {
        _repo.GetPagedAsync(null, null, 1, 20, default).Returns((new List<AccountPayable>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<AccountPayableResponse>>(Arg.Any<object>()).Returns(new List<AccountPayableResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAccountsPayableQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(null, null, 1, 20, default);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldReturnPayablesByStatus()
    {
        _repo.GetPagedAsync(AccountPayableStatus.Pending, null, 1, 20, default).Returns((new List<AccountPayable>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<AccountPayableResponse>>(Arg.Any<object>()).Returns(new List<AccountPayableResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAccountsPayableQuery(AccountPayableStatus.Pending), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(AccountPayableStatus.Pending, null, 1, 20, default);
    }

    [Fact]
    public async Task Handle_WithSupplierFilter_ShouldReturnPayablesBySupplier()
    {
        var supplierId = Guid.NewGuid();
        _repo.GetPagedAsync(null, supplierId, 1, 20, default).Returns((new List<AccountPayable>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<AccountPayableResponse>>(Arg.Any<object>()).Returns(new List<AccountPayableResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAccountsPayableQuery(SupplierId: supplierId), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(null, supplierId, 1, 20, default);
    }
}

public class GetAccountsReceivableQueryHandlerTests
{
    private readonly IAccountReceivableRepository _repo = Substitute.For<IAccountReceivableRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAccountsReceivableQueryHandler CreateHandler() => new(_repo, _mapper);
    [Fact]
    public async Task Handle_WithNoFilter_ShouldReturnPagedOpenReceivables()
    {
        _repo.GetPagedAsync(null, 1, 20, default).Returns((new List<AccountReceivable>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<AccountReceivableResponse>>(Arg.Any<object>()).Returns(new List<AccountReceivableResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAccountsReceivableQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(null, 1, 20, default);
    }
    [Fact]
    public async Task Handle_WithCustomerFilter_ShouldReturnPagedReceivablesByCustomer()
    {
        var customerId = Guid.NewGuid();
        _repo.GetPagedAsync(customerId, 1, 20, default).Returns((new List<AccountReceivable>().AsReadOnly(), 0));
        _mapper.Map<IReadOnlyList<AccountReceivableResponse>>(Arg.Any<object>()).Returns(new List<AccountReceivableResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAccountsReceivableQuery(customerId), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetPagedAsync(customerId, 1, 20, default);
    }
}

public class GetAllBankAccountsQueryHandlerTests
{
    private readonly IBankAccountRepository _repo = Substitute.For<IBankAccountRepository>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private GetAllBankAccountsQueryHandler CreateHandler() => new(_repo, _mapper);

    [Fact]
    public async Task Handle_ShouldReturnAllBankAccounts()
    {
        _repo.GetActiveAsync(default).Returns(new List<BankAccount>().AsReadOnly());
        _mapper.Map<IReadOnlyList<BankAccountResponse>>(Arg.Any<object>()).Returns(new List<BankAccountResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAllBankAccountsQuery(), default);
        result.Should().NotBeNull();
        await _repo.Received(1).GetActiveAsync(default);
    }

    [Fact]
    public async Task Handle_WhenEmpty_ShouldReturnEmptyList()
    {
        _repo.GetActiveAsync(default).Returns(new List<BankAccount>().AsReadOnly());
        _mapper.Map<IReadOnlyList<BankAccountResponse>>(Arg.Any<object>()).Returns(new List<BankAccountResponse>().AsReadOnly());
        var result = await CreateHandler().Handle(new GetAllBankAccountsQuery(), default);
        result.Should().BeEmpty();
    }
}




