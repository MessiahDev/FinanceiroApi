using AutoMapper;
using FinanceiroApi.Application.Commands.BankStatements.CancelBankStatement;
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

namespace FinanceiroApi.Application.Tests.Handlers.BankStatements;

public class CancelBankStatementCommandHandlerTests
{
    private readonly IBankStatementRepository _repo = Substitute.For<IBankStatementRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CancelBankStatementCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static BankStatement MakeStatement() =>
        BankStatement.Create(Guid.NewGuid(),
            new DateOnly(2024, 6, 30),
            new DateOnly(2024, 6, 1),
            new DateOnly(2024, 6, 30),
            1000m, 2000m);

    private static BankStatementSummaryResponse MakeResponse(BankStatement s) => new(
        s.Id, s.BankAccountId, "BB",
        s.PeriodStart, s.PeriodEnd,
        s.OpeningBalance.Amount, s.ClosingBalance.Amount,
        s.Status.ToString(), s.TotalEntries);

    [Fact]
    public async Task Handle_ImportedStatement_ShouldCancelAndReturnResponse()
    {
        var statement = MakeStatement();
        _repo.GetWithEntriesAsync(statement.Id, default).Returns(statement);
        _mapper.Map<BankStatementSummaryResponse>(Arg.Any<BankStatement>())
               .Returns(c => MakeResponse(c.Arg<BankStatement>()));

        var result = await CreateHandler().Handle(
            new CancelBankStatementCommand(statement.Id, "Importado errado"), default);

        result.Should().NotBeNull();
        statement.Status.Should().Be(BankStatementStatus.Cancelled);
        await _repo.Received(1).UpdateAsync(statement, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithEntriesAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new CancelBankStatementCommand(Guid.NewGuid(), "Motivo"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_ReconciledStatement_ShouldThrowDomainException()
    {
        var statement = MakeStatement();
        statement.MarkAsReconciled();
        _repo.GetWithEntriesAsync(statement.Id, default).Returns(statement);

        var act = () => CreateHandler().Handle(
            new CancelBankStatementCommand(statement.Id, "Erro"), default);

        await act.Should().ThrowAsync<Exception>();
    }
}
