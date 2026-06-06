using FinanceiroApi.Application.Commands.JournalEntries.CreateJournalEntry;
using FinanceiroApi.Application.Commands.JournalEntries.PostJournalEntry;
using FinanceiroApi.Application.Commands.JournalEntries.ReverseJournalEntry;
using FinanceiroApi.CrossCutting.Services;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.JournalEntries;

public class CreateJournalEntryCommandHandlerTests
{
    private readonly IJournalEntryRepository _journalRepo = Substitute.For<IJournalEntryRepository>();
    private readonly IAccountingPeriodRepository _periodRepo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IChartOfAccountRepository _accountRepo = Substitute.For<IChartOfAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();

    private CreateJournalEntryCommandHandler CreateHandler() =>
        new(_journalRepo, _periodRepo, _accountRepo, _uow, _currentUser);

    private static AccountingPeriod MakeOpenPeriod() => AccountingPeriod.Create(2024, 6);

    private static ChartOfAccount MakeAccount() => ChartOfAccount.Create(
        "1.1.01", "Caixa", null, AccountType.Asset, AccountNature.Debit, acceptsEntries: true);

    private static CreateJournalEntryCommand MakeCommand(Guid periodId, Guid accountId) => new(
        Description: "Lançamento teste",
        EntryDate: new DateTime(2024, 6, 15),
        EntryType: JournalEntryType.Manual,
        AccountingPeriodId: periodId,
        ReferenceDocument: null,
        ReferenceDocumentType: null,
        ReferenceDocumentId: null,
        Lines:
        [
            new CreateJournalEntryLineRequest(accountId, DebitCredit.Debit,  500m, null),
            new CreateJournalEntryLineRequest(accountId, DebitCredit.Credit, 500m, null)
        ]);

    [Fact]
    public async Task Handle_ValidEntry_ShouldCreateAndReturnId()
    {
        var period = MakeOpenPeriod();
        var account = MakeAccount();
        var userId = Guid.NewGuid();
        var cmd = MakeCommand(period.Id, account.Id);

        _currentUser.UserId.Returns(userId);
        _periodRepo.GetByIdAsync(period.Id, default).Returns(period);
        _accountRepo.GetByIdAsync(account.Id, default).Returns(account);
        _journalRepo.GetNextEntryNumberAsync(2024, default).Returns("2024/001");

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBe(Guid.Empty);
        await _journalRepo.Received(1).AddAsync(Arg.Any<JournalEntry>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_PeriodNotFound_ShouldThrowDomainException()
    {
        _periodRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var cmd = MakeCommand(Guid.NewGuid(), Guid.NewGuid());
        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_ClosedPeriod_ShouldThrowAccountingPeriodClosedException()
    {
        var period = MakeOpenPeriod();
        period.Close();
        _periodRepo.GetByIdAsync(period.Id, default).Returns(period);

        var cmd = MakeCommand(period.Id, Guid.NewGuid());
        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<AccountingPeriodClosedException>();
    }

    [Fact]
    public async Task Handle_EntryDateOutsidePeriod_ShouldThrowDomainException()
    {
        var period = MakeOpenPeriod(); // junho/2024
        var account = MakeAccount();
        _periodRepo.GetByIdAsync(period.Id, default).Returns(period);
        _accountRepo.GetByIdAsync(account.Id, default).Returns(account);
        _currentUser.UserId.Returns(Guid.NewGuid());

        // Data fora do período (julho)
        var cmd = new CreateJournalEntryCommand(
            "Teste", new DateTime(2024, 7, 1), JournalEntryType.Manual,
            period.Id, null, null, null,
            [new CreateJournalEntryLineRequest(account.Id, DebitCredit.Debit, 100m, null)]);

        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_AccountNotFound_ShouldThrowDomainException()
    {
        var period = MakeOpenPeriod();
        _periodRepo.GetByIdAsync(period.Id, default).Returns(period);
        _accountRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        _currentUser.UserId.Returns(Guid.NewGuid());
        _journalRepo.GetNextEntryNumberAsync(2024, default).Returns("2024/001");

        var cmd = MakeCommand(period.Id, Guid.NewGuid());
        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_AccountNotAcceptingEntries_ShouldThrowAccountNotAcceptingEntriesException()
    {
        var period = MakeOpenPeriod();
        var account = ChartOfAccount.Create(
            "1.1.02", "Grupo", null, AccountType.Asset, AccountNature.Debit, acceptsEntries: false);

        _periodRepo.GetByIdAsync(period.Id, default).Returns(period);
        _accountRepo.GetByIdAsync(account.Id, default).Returns(account);
        _currentUser.UserId.Returns(Guid.NewGuid());
        _journalRepo.GetNextEntryNumberAsync(2024, default).Returns("2024/001");

        var cmd = MakeCommand(period.Id, account.Id);
        var act = () => CreateHandler().Handle(cmd, default);

        await act.Should().ThrowAsync<AccountNotAcceptingEntriesException>();
    }
}

public class PostJournalEntryCommandHandlerTests
{
    private readonly IJournalEntryRepository _repo = Substitute.For<IJournalEntryRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private PostJournalEntryCommandHandler CreateHandler() => new(_repo, _uow);

    private static JournalEntry MakeDraftEntry()
    {
        var entry = JournalEntry.Create("2024/001", "Teste", DateTime.UtcNow,
            JournalEntryType.Manual, Guid.NewGuid(), Guid.NewGuid());
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 500m, null);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 500m, null);
        return entry;
    }

    [Fact]
    public async Task Handle_DraftEntry_ShouldPostSuccessfully()
    {
        var entry = MakeDraftEntry();
        _repo.GetWithLinesAsync(entry.Id, default).Returns(entry);

        await CreateHandler().Handle(new PostJournalEntryCommand(entry.Id), default);

        entry.Status.Should().Be(JournalEntryStatus.Posted);
        await _repo.Received(1).UpdateAsync(entry, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_EntryNotFound_ShouldThrowDomainException()
    {
        _repo.GetWithLinesAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var act = () => CreateHandler().Handle(new PostJournalEntryCommand(Guid.NewGuid()), default);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_UnbalancedEntry_ShouldThrowDomainException()
    {
        var entry = JournalEntry.Create("2024/002", "Desequilibrado", DateTime.UtcNow,
            JournalEntryType.Manual, Guid.NewGuid(), Guid.NewGuid());
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 300m, null);
        // sem linha de crédito — desequilibrado
        _repo.GetWithLinesAsync(entry.Id, default).Returns(entry);

        var act = () => CreateHandler().Handle(new PostJournalEntryCommand(entry.Id), default);

        await act.Should().ThrowAsync<DomainException>();
    }
}

public class ReverseJournalEntryCommandHandlerTests
{
    private readonly IJournalEntryRepository _journalRepo = Substitute.For<IJournalEntryRepository>();
    private readonly IAccountingPeriodRepository _periodRepo = Substitute.For<IAccountingPeriodRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private ReverseJournalEntryCommandHandler CreateHandler() =>
        new(_journalRepo, _periodRepo, _uow);

    private static JournalEntry MakePostedEntry()
    {
        var entry = JournalEntry.Create("2024/001", "Lançamento original", DateTime.UtcNow,
            JournalEntryType.Manual, Guid.NewGuid(), Guid.NewGuid());
        entry.AddLine(Guid.NewGuid(), DebitCredit.Debit, 500m, null);
        entry.AddLine(Guid.NewGuid(), DebitCredit.Credit, 500m, null);
        entry.Post();
        return entry;
    }

    [Fact]
    public async Task Handle_PostedEntry_ShouldCreateReversalAndReturnId()
    {
        var original = MakePostedEntry();
        var openPeriod = AccountingPeriod.Create(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
        var reversedById = Guid.NewGuid();

        _journalRepo.GetWithLinesAsync(original.Id, default).Returns(original);
        _periodRepo.GetCurrentOpenPeriodAsync(default).Returns(openPeriod);
        _journalRepo.GetNextEntryNumberAsync(DateTime.UtcNow.Year, default).Returns("2024/002");

        var cmd = new ReverseJournalEntryCommand(original.Id, "Estorno", reversedById);
        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBe(Guid.Empty);
        original.Status.Should().Be(JournalEntryStatus.Reversed);
        await _journalRepo.Received(1).AddAsync(Arg.Any<JournalEntry>(), default);
        await _journalRepo.Received(1).UpdateAsync(original, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_EntryNotFound_ShouldThrowDomainException()
    {
        _journalRepo.GetWithLinesAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var act = () => CreateHandler().Handle(
            new ReverseJournalEntryCommand(Guid.NewGuid(), "Estorno", Guid.NewGuid()), default);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_NoOpenPeriod_ShouldThrowDomainException()
    {
        var original = MakePostedEntry();
        _journalRepo.GetWithLinesAsync(original.Id, default).Returns(original);
        _periodRepo.GetCurrentOpenPeriodAsync(default).ReturnsNull();

        var act = () => CreateHandler().Handle(
            new ReverseJournalEntryCommand(original.Id, "Estorno", Guid.NewGuid()), default);

        await act.Should().ThrowAsync<DomainException>();
    }
}
