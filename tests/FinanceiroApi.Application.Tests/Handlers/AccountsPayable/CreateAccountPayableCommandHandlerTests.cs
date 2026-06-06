using AutoMapper;
using FinanceiroApi.Application.Commands.AccountsPayable.CancelAccountPayable;
using FinanceiroApi.Application.Commands.AccountsPayable.CreateAccountPayable;
using FinanceiroApi.Application.Commands.AccountsPayable.PayAccountPayable;
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

namespace FinanceiroApi.Application.Tests.Handlers.AccountsPayable;

public class CreateAccountPayableCommandHandlerTests
{
    private readonly IAccountPayableRepository _payableRepo = Substitute.For<IAccountPayableRepository>();
    private readonly ISupplierRepository _supplierRepo = Substitute.For<ISupplierRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateAccountPayableCommandHandler CreateHandler() =>
        new(_payableRepo, _supplierRepo, _uow, _mapper, _notif);

    private static AccountPayableResponse MakeResponse(AccountPayable ap) =>
        new(ap.Id, ap.SupplierId, "Fornecedor", null, null, ap.Description,
            ap.TotalAmount.Amount, 0m, ap.TotalAmount.Amount, "BRL",
            ap.DueDate, null, "Pending", null, null, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_ValidSupplier_ShouldCreatePayable()
    {
        var supplierId = Guid.NewGuid();
        var cmd = new CreateAccountPayableCommand(supplierId, "NF 001", 1000m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), null, null, null);

        _supplierRepo.ExistsAsync(supplierId, default).Returns(true);
        _mapper.Map<AccountPayableResponse>(Arg.Any<AccountPayable>())
               .Returns(c => MakeResponse(c.Arg<AccountPayable>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        await _payableRepo.Received(1).AddAsync(Arg.Any<AccountPayable>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_SupplierNotFound_ShouldNotifyAndReturnNull()
    {
        _supplierRepo.ExistsAsync(Arg.Any<Guid>(), default).Returns(false);

        var cmd = new CreateAccountPayableCommand(Guid.NewGuid(), "NF 001", 1000m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), null, null, null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("SupplierId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class CancelAccountPayableCommandHandlerTests
{
    private readonly IAccountPayableRepository _repo = Substitute.For<IAccountPayableRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CancelAccountPayableCommandHandler CreateHandler() =>
        new(_repo, _uow, _mapper, _notif);

    private static AccountPayableResponse MakeResponse(AccountPayable ap) =>
        new(ap.Id, ap.SupplierId, "Fornecedor", null, null, ap.Description,
            ap.TotalAmount.Amount, 0m, ap.TotalAmount.Amount, "BRL",
            ap.DueDate, null, "Cancelled", null, null, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_ExistingPayable_ShouldCancel()
    {
        var payable = AccountPayable.Create(Guid.NewGuid(), "Desc", 500m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));

        _repo.GetByIdAsync(payable.Id, default).Returns(payable);
        _mapper.Map<AccountPayableResponse>(Arg.Any<AccountPayable>())
               .Returns(c => MakeResponse(c.Arg<AccountPayable>()));

        var result = await CreateHandler().Handle(new CancelAccountPayableCommand(payable.Id, "Motivo"), default);

        payable.Status.Should().Be(AccountPayableStatus.Cancelled);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(new CancelAccountPayableCommand(Guid.NewGuid(), "Motivo"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }
}

public class PayAccountPayableCommandHandlerTests
{
    private readonly IAccountPayableRepository _payableRepo = Substitute.For<IAccountPayableRepository>();
    private readonly IBankAccountRepository _bankRepo = Substitute.For<IBankAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private PayAccountPayableCommandHandler CreateHandler() =>
        new(_payableRepo, _bankRepo, _uow, _mapper, _notif);

    private static AccountPayableResponse MakeResponse(AccountPayable ap) =>
        new(ap.Id, ap.SupplierId, "Fornecedor", null, null, ap.Description,
            ap.TotalAmount.Amount, ap.TotalAmount.Amount, 0m, "BRL",
            ap.DueDate, null, "Paid", null, null, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_ValidData_ShouldRegisterPaymentAndDebitBank()
    {
        var payable = AccountPayable.Create(Guid.NewGuid(), "Desc", 500m,
                              DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));
        var bankAccount = BankAccount.Create("Banco", "001", "1234", "00001-0", BankAccountType.Checking, 1000m);
        var cmd = new PayAccountPayableCommand(payable.Id, 500m,
                              DateOnly.FromDateTime(DateTime.UtcNow), bankAccount.Id);

        _payableRepo.GetByIdAsync(payable.Id, default).Returns(payable);
        _bankRepo.GetByIdAsync(bankAccount.Id, default).Returns(bankAccount);
        _mapper.Map<AccountPayableResponse>(Arg.Any<AccountPayable>())
               .Returns(c => MakeResponse(c.Arg<AccountPayable>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        payable.Status.Should().Be(AccountPayableStatus.Paid);
        bankAccount.Balance.Amount.Should().Be(500m);
    }

    [Fact]
    public async Task Handle_PayableNotFound_ShouldNotify()
    {
        _payableRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new PayAccountPayableCommand(Guid.NewGuid(), 100m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()),
            default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_BankNotFound_ShouldNotify()
    {
        var payable = AccountPayable.Create(Guid.NewGuid(), "Desc", 500m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)));

        _payableRepo.GetByIdAsync(payable.Id, default).Returns(payable);
        _bankRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new PayAccountPayableCommand(payable.Id, 100m, DateOnly.FromDateTime(DateTime.UtcNow), Guid.NewGuid()),
            default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("BankAccountId", Arg.Any<string>());
    }
}
