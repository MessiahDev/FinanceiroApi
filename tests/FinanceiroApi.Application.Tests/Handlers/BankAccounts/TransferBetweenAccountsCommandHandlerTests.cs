using AutoMapper;
using FinanceiroApi.Application.Commands.BankAccounts.TransferBetweenAccounts;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.BankAccounts;

public class TransferBetweenAccountsCommandHandlerTests
{
    private readonly IBankAccountRepository _repo = Substitute.For<IBankAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private TransferBetweenAccountsCommandHandler CreateHandler() =>
        new(_repo, _uow, _notif);

    private static BankAccount MakeAccount(decimal balance = 1000m) =>
        BankAccount.Create("BB", "001", "1234", "56789-0", BankAccountType.Checking, balance);

    [Fact]
    public async Task Handle_WithValidAccounts_ShouldTransferAndReturnTrue()
    {
        var source = MakeAccount(1000m);
        var dest = MakeAccount(0m);
        var cmd = new TransferBetweenAccountsCommand(source.Id, dest.Id, 500m, "transferencia");

        _repo.GetByIdAsync(source.Id, default).Returns(source);
        _repo.GetByIdAsync(dest.Id, default).Returns(dest);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeTrue();
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_SameSourceAndDestination_ShouldAddNotificationAndReturnFalse()
    {
        var id = Guid.NewGuid();
        var cmd = new TransferBetweenAccountsCommand(id, id, 500m, "mesmo");

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("AccountId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentSourceAccount_ShouldAddNotificationAndReturnFalse()
    {
        var cmd = new TransferBetweenAccountsCommand(Guid.NewGuid(), Guid.NewGuid(), 500m, "x");
        _repo.GetByIdAsync(cmd.SourceAccountId, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("SourceAccountId", Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WithNonExistentDestinationAccount_ShouldAddNotificationAndReturnFalse()
    {
        var source = MakeAccount();
        var cmd = new TransferBetweenAccountsCommand(source.Id, Guid.NewGuid(), 500m, "x");

        _repo.GetByIdAsync(source.Id, default).Returns(source);
        _repo.GetByIdAsync(cmd.DestinationAccountId, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeFalse();
        _notif.Received(1).AddNotification("DestinationAccountId", Arg.Any<string>());
    }
}
