using AutoMapper;
using FinanceiroApi.Application.Commands.BankStatements.ImportBankStatement;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.BankStatements;

public class ImportBankStatementCommandHandlerTests
{
    private readonly IBankStatementRepository _statementRepo = Substitute.For<IBankStatementRepository>();
    private readonly IBankAccountRepository _bankAccountRepo = Substitute.For<IBankAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private ImportBankStatementCommandHandler CreateHandler() =>
        new(_statementRepo, _bankAccountRepo, _uow, _mapper, _notif);

    private static ImportBankStatementCommand MakeCommand(Guid bankAccountId) =>
        new(bankAccountId,
            DateOnly.FromDateTime(DateTime.Today),
            new DateOnly(2025, 1, 1),
            new DateOnly(2025, 1, 31),
            1000m, 1500m, "extrato-jan-2025.ofx", null,
            [new ImportBankStatementEntryCommand(new DateOnly(2025, 1, 10), "Pix recebido", 500m, BankStatementEntryType.Credit, null)]);

    [Fact]
    public async Task Handle_WithValidData_ShouldImportAndReturnResponse()
    {
        var bankAccountId = Guid.NewGuid();
        var cmd = MakeCommand(bankAccountId);
        var expected = new BankStatementResponse(Guid.NewGuid(), bankAccountId, "Banco",
            cmd.StatementDate, cmd.PeriodStart, cmd.PeriodEnd, 1000m, 1500m, "BRL",
            "Imported", 1, 500m, 0m, cmd.FileName, null, DateTime.UtcNow, null, []);

        _bankAccountRepo.ExistsAsync(bankAccountId, default).Returns(true);
        _statementRepo.ExistsForPeriodAsync(bankAccountId, cmd.PeriodStart, cmd.PeriodEnd, default).Returns(false);
        _statementRepo.GetWithEntriesAsync(Arg.Any<Guid>(), default)
            .Returns(FinanceiroApi.Domain.Entities.BankStatement.Create(bankAccountId,
                cmd.StatementDate, cmd.PeriodStart, cmd.PeriodEnd, 1000m, 1500m, null, null));
        _mapper.Map<BankStatementResponse>(Arg.Any<object>()).Returns(expected);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _statementRepo.Received(1).AddAsync(Arg.Any<FinanceiroApi.Domain.Entities.BankStatement>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentBankAccount_ShouldNotifyAndReturnNull()
    {
        var cmd = MakeCommand(Guid.NewGuid());
        _bankAccountRepo.ExistsAsync(cmd.BankAccountId, default).Returns(false);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("BankAccountId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithDuplicatePeriod_ShouldNotifyAndReturnNull()
    {
        var bankAccountId = Guid.NewGuid();
        var cmd = MakeCommand(bankAccountId);
        _bankAccountRepo.ExistsAsync(bankAccountId, default).Returns(true);
        _statementRepo.ExistsForPeriodAsync(bankAccountId, cmd.PeriodStart, cmd.PeriodEnd, default).Returns(true);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Period", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}
