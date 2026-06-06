using AutoMapper;
using FinanceiroApi.Application.Commands.BankReconciliations.AddReconciliationItem;
using FinanceiroApi.Application.Commands.BankReconciliations.CancelReconciliation;
using FinanceiroApi.Application.Commands.BankReconciliations.CompleteReconciliation;
using FinanceiroApi.Application.Commands.BankReconciliations.CreateBankReconciliation;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.BankReconciliations;

public class CreateBankReconciliationCommandHandlerTests
{
    private readonly IBankReconciliationRepository _reconcRepo = Substitute.For<IBankReconciliationRepository>();
    private readonly IBankStatementRepository _statementRepo = Substitute.For<IBankStatementRepository>();
    private readonly IBankAccountRepository _bankRepo = Substitute.For<IBankAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateBankReconciliationCommandHandler CreateHandler() =>
        new(_reconcRepo, _statementRepo, _bankRepo, _uow, _mapper, _notif);

    private static BankReconciliationResponse MakeResponse(BankReconciliation r) => new(
        r.Id, r.BankAccountId, "BB", r.BankStatementId,
        r.PeriodStart, r.PeriodEnd,
        r.StatementOpeningBalance.Amount, r.StatementClosingBalance.Amount,
        r.SystemBalance.Amount, r.Difference.Amount, r.IsBalanced,
        r.Status.ToString(), r.TotalItems, r.MatchedItems, r.UnmatchedItems,
        r.CompletedAt, r.CompletedBy, r.Notes, DateTime.UtcNow, null, []);

    [Fact]
    public async Task Handle_ValidData_ShouldCreateAndReturnResponse()
    {
        var bankId = Guid.NewGuid();
        var statementId = Guid.NewGuid();
        var statement = BankStatement.Create(bankId, new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30), 1000m, 2000m);
        var reconciliation = BankReconciliation.Create(bankId, statementId,
            new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30), 1000m, 2000m, 2000m);

        _bankRepo.ExistsAsync(bankId, default).Returns(true);
        _statementRepo.GetWithEntriesAsync(statementId, default).Returns(statement);
        _reconcRepo.ExistsForStatementAsync(statementId, default).Returns(false);
        _reconcRepo.GetWithItemsAsync(Arg.Any<Guid>(), default).Returns(reconciliation);
        _mapper.Map<BankReconciliationResponse>(Arg.Any<BankReconciliation>())
               .Returns(c => MakeResponse(c.Arg<BankReconciliation>()));

        var cmd = new CreateBankReconciliationCommand(bankId, statementId, 2000m, null);
        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _reconcRepo.Received(1).AddAsync(Arg.Any<BankReconciliation>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_BankAccountNotFound_ShouldNotifyAndReturnNull()
    {
        _bankRepo.ExistsAsync(Arg.Any<Guid>(), default).Returns(false);
        var cmd = new CreateBankReconciliationCommand(Guid.NewGuid(), Guid.NewGuid(), 0m, null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("BankAccountId", Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_StatementAlreadyReconciled_ShouldNotifyAndReturnNull()
    {
        var bankId = Guid.NewGuid();
        var statementId = Guid.NewGuid();
        var statement = BankStatement.Create(bankId, new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30), 1000m, 2000m);

        _bankRepo.ExistsAsync(bankId, default).Returns(true);
        _statementRepo.GetWithEntriesAsync(statementId, default).Returns(statement);
        _reconcRepo.ExistsForStatementAsync(statementId, default).Returns(true);

        var cmd = new CreateBankReconciliationCommand(bankId, statementId, 2000m, null);
        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("BankStatementId", Arg.Any<string>());
    }
}

public class AddReconciliationItemCommandHandlerTests
{
    private readonly IBankReconciliationRepository _repo = Substitute.For<IBankReconciliationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private AddReconciliationItemCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static BankReconciliation MakeReconciliation() =>
        BankReconciliation.Create(Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30), 1000m, 2000m, 2000m);

    private static BankReconciliationResponse MakeResponse(BankReconciliation r) => new(
        r.Id, r.BankAccountId, "BB", r.BankStatementId,
        r.PeriodStart, r.PeriodEnd,
        r.StatementOpeningBalance.Amount, r.StatementClosingBalance.Amount,
        r.SystemBalance.Amount, r.Difference.Amount, r.IsBalanced,
        r.Status.ToString(), r.TotalItems, r.MatchedItems, r.UnmatchedItems,
        r.CompletedAt, r.CompletedBy, r.Notes, DateTime.UtcNow, null, []);

    [Fact]
    public async Task Handle_ExistingReconciliation_ShouldAddItemAndReturnResponse()
    {
        var reconciliation = MakeReconciliation();
        var cmd = new AddReconciliationItemCommand(
            reconciliation.Id, Guid.NewGuid(), null, 100m,
            ReconciliationItemStatus.Matched, null);

        _repo.GetWithItemsAsync(reconciliation.Id, default).Returns(reconciliation);
        _repo.GetWithItemsAsync(reconciliation.Id, default).Returns(reconciliation);
        _mapper.Map<BankReconciliationResponse>(Arg.Any<BankReconciliation>())
               .Returns(c => MakeResponse(c.Arg<BankReconciliation>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        reconciliation.TotalItems.Should().Be(1);
        await _repo.Received(1).UpdateAsync(reconciliation, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithItemsAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var cmd = new AddReconciliationItemCommand(
            Guid.NewGuid(), Guid.NewGuid(), null, 100m,
            ReconciliationItemStatus.Matched, null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("ReconciliationId", Arg.Any<string>());
    }
}

public class CompleteReconciliationCommandHandlerTests
{
    private readonly IBankReconciliationRepository _repo = Substitute.For<IBankReconciliationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CompleteReconciliationCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static BankReconciliationResponse MakeResponse(BankReconciliation r) => new(
        r.Id, r.BankAccountId, "BB", r.BankStatementId,
        r.PeriodStart, r.PeriodEnd,
        r.StatementOpeningBalance.Amount, r.StatementClosingBalance.Amount,
        r.SystemBalance.Amount, r.Difference.Amount, r.IsBalanced,
        r.Status.ToString(), r.TotalItems, r.MatchedItems, r.UnmatchedItems,
        r.CompletedAt, r.CompletedBy, r.Notes, DateTime.UtcNow, null, []);

    [Fact]
    public async Task Handle_ReconciliationWithAllItemsMatched_ShouldCompleteAndReturnResponse()
    {
        var reconciliation = BankReconciliation.Create(Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30), 1000m, 2000m, 2000m);
        reconciliation.AddItem(Guid.NewGuid(), null, 100m, ReconciliationItemStatus.Matched, null);
        var completedBy = Guid.NewGuid();

        _repo.GetWithItemsAsync(reconciliation.Id, default).Returns(reconciliation);
        _mapper.Map<BankReconciliationResponse>(Arg.Any<BankReconciliation>())
               .Returns(c => MakeResponse(c.Arg<BankReconciliation>()));

        var result = await CreateHandler().Handle(
            new CompleteReconciliationCommand(reconciliation.Id, completedBy), default);

        result.Should().NotBeNull();
        reconciliation.Status.Should().Be(ReconciliationStatus.Completed);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithItemsAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new CompleteReconciliationCommand(Guid.NewGuid(), Guid.NewGuid()), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class CancelReconciliationCommandHandlerTests
{
    private readonly IBankReconciliationRepository _repo = Substitute.For<IBankReconciliationRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CancelReconciliationCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static BankReconciliationResponse MakeResponse(BankReconciliation r) => new(
        r.Id, r.BankAccountId, "BB", r.BankStatementId,
        r.PeriodStart, r.PeriodEnd,
        r.StatementOpeningBalance.Amount, r.StatementClosingBalance.Amount,
        r.SystemBalance.Amount, r.Difference.Amount, r.IsBalanced,
        r.Status.ToString(), r.TotalItems, r.MatchedItems, r.UnmatchedItems,
        r.CompletedAt, r.CompletedBy, r.Notes, DateTime.UtcNow, null, []);

    [Fact]
    public async Task Handle_OpenReconciliation_ShouldCancelAndReturnResponse()
    {
        var reconciliation = BankReconciliation.Create(Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2024, 6, 1), new DateOnly(2024, 6, 30), 1000m, 2000m, 2000m);
        _repo.GetWithItemsAsync(reconciliation.Id, default).Returns(reconciliation);
        _mapper.Map<BankReconciliationResponse>(Arg.Any<BankReconciliation>())
               .Returns(c => MakeResponse(c.Arg<BankReconciliation>()));

        var result = await CreateHandler().Handle(
            new CancelReconciliationCommand(reconciliation.Id, "Erro"), default);

        result.Should().NotBeNull();
        reconciliation.Status.Should().Be(ReconciliationStatus.Cancelled);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithItemsAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new CancelReconciliationCommand(Guid.NewGuid(), "Erro"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

