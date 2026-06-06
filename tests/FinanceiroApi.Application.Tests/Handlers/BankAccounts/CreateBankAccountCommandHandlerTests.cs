using AutoMapper;
using FinanceiroApi.Application.Commands.BankAccounts.CreateBankAccount;
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

namespace FinanceiroApi.Application.Tests.Handlers.BankAccounts;

public class CreateBankAccountCommandHandlerTests
{
    private readonly IBankAccountRepository _repo = Substitute.For<IBankAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateBankAccountCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static BankAccountResponse MakeResponse(BankAccount b) => new(
        b.Id, b.BankName, b.BankCode, b.Agency, b.AccountNumber,
        b.AccountType.ToString(), b.PixKey, b.Balance.Amount, "BRL",
        b.IsActive, b.Description, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_NewAccountNumber_ShouldCreateAndReturnResponse()
    {
        var cmd = new CreateBankAccountCommand(
            "Banco do Brasil", "001", "1234", "56789-0",
            BankAccountType.Checking, 1000m, null, null);

        _repo.GetByAccountNumberAsync("56789-0", default).ReturnsNull();
        _mapper.Map<BankAccountResponse>(Arg.Any<BankAccount>())
               .Returns(c => MakeResponse(c.Arg<BankAccount>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.AccountNumber.Should().Be("56789-0");
        await _repo.Received(1).AddAsync(Arg.Any<BankAccount>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateAccountNumber_ShouldNotifyAndReturnNull()
    {
        var existing = BankAccount.Create("BB", "001", "1234", "56789-0", BankAccountType.Checking, 0m);
        _repo.GetByAccountNumberAsync("56789-0", default).Returns(existing);

        var cmd = new CreateBankAccountCommand(
            "Banco do Brasil", "001", "1234", "56789-0",
            BankAccountType.Checking, 0m, null, null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("AccountNumber", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}
