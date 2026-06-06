using FinanceiroApi.Application.Commands.AccountingPeriods.CloseAccountingPeriod;
using FinanceiroApi.Application.Commands.AccountingPeriods.CreateAccountingPeriod;
using FinanceiroApi.Application.Commands.AccountingPeriods.LockAccountingPeriod;
using FinanceiroApi.Application.Commands.AccountingPeriods.ReopenAccountingPeriod;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.AccountingPeriods;

public class CreateAccountingPeriodCommandHandlerTests
{
    private readonly IAccountingPeriodRepository _repo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private CreateAccountingPeriodCommandHandler CreateHandler() => new(_repo, _uow);

    [Fact]
    public async Task Handle_NewPeriod_ShouldCreateAndReturnId()
    {
        _repo.ExistsByYearMonthAsync(2024, 6, null, default).Returns(false);

        var result = await CreateHandler().Handle(new CreateAccountingPeriodCommand(2024, 6), default);

        result.Should().NotBe(Guid.Empty);
        await _repo.Received(1).AddAsync(Arg.Any<AccountingPeriod>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicatePeriod_ShouldThrowDomainException()
    {
        _repo.ExistsByYearMonthAsync(2024, 6, null, default).Returns(true);

        var act = () => CreateHandler().Handle(new CreateAccountingPeriodCommand(2024, 6), default);

        await act.Should().ThrowAsync<DuplicateAccountingPeriodException>();
    }
}

public class CloseAccountingPeriodCommandHandlerTests
{
    private readonly IAccountingPeriodRepository _repo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ExistingOpenPeriod_ShouldClose()
    {
        var period = AccountingPeriod.Create(2024, 6);
        _repo.GetByIdAsync(period.Id, default).Returns(period);

        var handler = new CloseAccountingPeriodCommandHandler(_repo, _uow);
        await handler.Handle(new CloseAccountingPeriodCommand(period.Id), default);

        period.Status.Should().Be(AccountingPeriodStatus.Closed);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrowDomainException()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var handler = new CloseAccountingPeriodCommandHandler(_repo, _uow);
        var act = () => handler.Handle(new CloseAccountingPeriodCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<DomainException>();
    }
}

public class LockAccountingPeriodCommandHandlerTests
{
    private readonly IAccountingPeriodRepository _repo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ClosedPeriod_ShouldLock()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();
        _repo.GetByIdAsync(period.Id, default).Returns(period);

        var handler = new LockAccountingPeriodCommandHandler(_repo, _uow);
        await handler.Handle(new LockAccountingPeriodCommand(period.Id), default);

        period.Status.Should().Be(AccountingPeriodStatus.Locked);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldThrow()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var handler = new LockAccountingPeriodCommandHandler(_repo, _uow);
        var act = () => handler.Handle(new LockAccountingPeriodCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<DomainException>();
    }
}

public class ReopenAccountingPeriodCommandHandlerTests
{
    private readonly IAccountingPeriodRepository _repo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Handle_ClosedPeriod_ShouldReopen()
    {
        var period = AccountingPeriod.Create(2024, 6);
        period.Close();
        _repo.GetByIdAsync(period.Id, default).Returns(period);

        var handler = new ReopenAccountingPeriodCommandHandler(_repo, _uow);
        await handler.Handle(new ReopenAccountingPeriodCommand(period.Id), default);

        period.Status.Should().Be(AccountingPeriodStatus.Open);
        await _uow.Received(1).CommitAsync(default);
    }
}
