using AutoMapper;
using FinanceiroApi.Application.Commands.AccountsReceivable.CancelAccountReceivable;
using FinanceiroApi.Application.Commands.AccountsReceivable.ReceivePayment;
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

namespace FinanceiroApi.Application.Tests.Handlers.AccountsReceivable;

public class CancelAccountReceivableCommandHandlerTests
{
    private readonly IAccountReceivableRepository _repo = Substitute.For<IAccountReceivableRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CancelAccountReceivableCommandHandler CreateHandler() =>
        new(_repo, _uow, _mapper, _notif);

    private static AccountReceivable MakeReceivable() =>
        AccountReceivable.Create(Guid.NewGuid(), "Venda", 1000m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));

    private static AccountReceivableResponse MakeResponse(AccountReceivable ar) =>
        new(ar.Id, ar.CustomerId, "Cliente", null, null, ar.Description,
            ar.TotalAmount.Amount, 0m, ar.TotalAmount.Amount, "BRL",
            ar.DueDate, null, "Cancelled", null, null, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_WithValidReceivable_ShouldCancelAndReturnResponse()
    {
        var ar = MakeReceivable();
        var cmd = new CancelAccountReceivableCommand(ar.Id, "erro");
        _repo.GetByIdAsync(ar.Id, default).Returns(ar);
        _mapper.Map<AccountReceivableResponse>(ar).Returns(MakeResponse(ar));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentReceivable_ShouldReturnNull()
    {
        var cmd = new CancelAccountReceivableCommand(Guid.NewGuid(), "erro");
        _repo.GetByIdAsync(cmd.Id, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class ReceivePaymentCommandHandlerTests
{
    private readonly IAccountReceivableRepository _arRepo = Substitute.For<IAccountReceivableRepository>();
    private readonly IBankAccountRepository _bankRepo = Substitute.For<IBankAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private ReceivePaymentCommandHandler CreateHandler() =>
        new(_arRepo, _bankRepo, _uow, _mapper, _notif);

    [Fact]
    public async Task Handle_WithValidData_ShouldRegisterReceiptAndReturnResponse()
    {
        var ar = AccountReceivable.Create(Guid.NewGuid(), "Venda", 1000m,
                          DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        var bank = BankAccount.Create("BB", "001", "1234", "56789-0", BankAccountType.Checking, 0m);
        var cmd = new ReceivePaymentCommand(ar.Id, 1000m,
                          DateOnly.FromDateTime(DateTime.UtcNow), bank.Id);
        var expected = new AccountReceivableResponse(ar.Id, ar.CustomerId, "Cliente",
            null, null, ar.Description, 1000m, 1000m, 0m, "BRL",
            ar.DueDate, cmd.ReceiptDate, "Received", null, null, DateTime.UtcNow, null);

        _arRepo.GetByIdAsync(ar.Id, default).Returns(ar);
        _bankRepo.GetByIdAsync(bank.Id, default).Returns(bank);
        _mapper.Map<AccountReceivableResponse>(ar).Returns(expected);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_WithNonExistentReceivable_ShouldReturnNull()
    {
        var cmd = new ReceivePaymentCommand(Guid.NewGuid(), 500m,
            DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());
        _arRepo.GetByIdAsync(cmd.Id, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WithNonExistentBankAccount_ShouldReturnNull()
    {
        var ar = AccountReceivable.Create(Guid.NewGuid(), "Venda", 1000m,
                      DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)));
        var cmd = new ReceivePaymentCommand(ar.Id, 500m,
                      DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid());

        _arRepo.GetByIdAsync(ar.Id, default).Returns(ar);
        _bankRepo.GetByIdAsync(cmd.BankAccountId, default).ReturnsNull();

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("BankAccountId", Arg.Any<string>());
    }
}
