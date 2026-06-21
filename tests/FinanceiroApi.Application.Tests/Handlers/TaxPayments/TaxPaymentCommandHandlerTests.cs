using AutoMapper;
using FinanceiroApi.Application.Commands.TaxPayments.CancelTaxPayment;
using FinanceiroApi.Application.Commands.TaxPayments.CreateTaxPayment;
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

namespace FinanceiroApi.Application.Tests.Handlers.TaxPayments;

public class CreateTaxPaymentCommandHandlerTests
{
    private readonly ITaxPaymentRepository _taxPaymentRepo = Substitute.For<ITaxPaymentRepository>();
    private readonly ITaxEntryRepository _taxEntryRepo = Substitute.For<ITaxEntryRepository>();
    private readonly IBankAccountRepository _bankRepo = Substitute.For<IBankAccountRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateTaxPaymentCommandHandler CreateHandler() =>
        new(_taxPaymentRepo, _taxEntryRepo, _bankRepo, _uow, _mapper, _notif);

    private static TaxEntry MakeEntry() => TaxEntry.Create(
        TaxType.ISS, "ISS", 10000m, 5m,
        new DateOnly(2024, 6, 1), new DateOnly(2024, 7, 15));

    private static TaxPaymentResponse MakeResponse(TaxPayment p) => new(
        p.Id, p.TaxEntryId, "ISS", p.BankAccountId, "BB",
        p.Amount.Amount, p.Fine.Amount, p.Interest.Amount, p.TotalPaid.Amount,
        "BRL", p.PaymentDate, p.DarfNumber, p.ReceiptCode, p.Status.ToString(),
        p.Notes, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_ValidPayment_ShouldCreateAndReturnResponse()
    {
        var entry = MakeEntry();
        var bankId = Guid.NewGuid();
        var payment = TaxPayment.Create(entry.Id, bankId, 500m, DateOnly.FromDateTime(DateTime.UtcNow));
        var cmd = new CreateTaxPaymentCommand(
            entry.Id, bankId, 500m, DateOnly.FromDateTime(DateTime.UtcNow),
            0m, 0m, null, null, null);

        var bankAccount = BankAccount.Create("Bradesco", "237", "1234", "56789-0", BankAccountType.Checking, 10000m);

        _taxEntryRepo.GetByIdAsync(entry.Id, default).Returns(entry);
        _bankRepo.GetByIdAsync(bankId, default).Returns(bankAccount);
        _taxPaymentRepo.GetWithDetailsAsync(Arg.Any<Guid>(), default).Returns(payment);
        _mapper.Map<TaxPaymentResponse>(Arg.Any<TaxPayment>())
               .Returns(c => MakeResponse(c.Arg<TaxPayment>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        entry.Status.Should().Be(TaxEntryStatus.Paid);
        await _taxPaymentRepo.Received(1).AddAsync(Arg.Any<TaxPayment>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_TaxEntryNotFound_ShouldNotifyAndReturnNull()
    {
        _taxEntryRepo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();
        var cmd = new CreateTaxPaymentCommand(
            Guid.NewGuid(), Guid.NewGuid(), 500m,
            DateOnly.FromDateTime(DateTime.UtcNow), 0m, 0m, null, null, null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("TaxEntryId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_BankAccountNotFound_ShouldNotifyAndReturnNull()
    {
        var entry = MakeEntry();
        _taxEntryRepo.GetByIdAsync(entry.Id, default).Returns(entry);
        _bankRepo.ExistsAsync(Arg.Any<Guid>(), default).Returns(false);

        var cmd = new CreateTaxPaymentCommand(
            entry.Id, Guid.NewGuid(), 500m,
            DateOnly.FromDateTime(DateTime.UtcNow), 0m, 0m, null, null, null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("BankAccountId", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class CancelTaxPaymentCommandHandlerTests
{
    private readonly ITaxPaymentRepository _repo = Substitute.For<ITaxPaymentRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CancelTaxPaymentCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static TaxPayment MakePayment() =>
        TaxPayment.Create(Guid.NewGuid(), Guid.NewGuid(), 500m,
            DateOnly.FromDateTime(DateTime.UtcNow));

    private static TaxPaymentResponse MakeResponse(TaxPayment p) => new(
        p.Id, p.TaxEntryId, "ISS", p.BankAccountId, "BB",
        p.Amount.Amount, p.Fine.Amount, p.Interest.Amount, p.TotalPaid.Amount,
        "BRL", p.PaymentDate, p.DarfNumber, p.ReceiptCode, p.Status.ToString(),
        p.Notes, DateTime.UtcNow, null);

    [Fact]
    public async Task Handle_ExistingPayment_ShouldCancelAndReturnResponse()
    {
        var payment = MakePayment();
        _repo.GetWithDetailsAsync(payment.Id, default).Returns(payment);
        _mapper.Map<TaxPaymentResponse>(Arg.Any<TaxPayment>())
               .Returns(c => MakeResponse(c.Arg<TaxPayment>()));

        var result = await CreateHandler().Handle(
            new CancelTaxPaymentCommand(payment.Id, "Erro"), default);

        result.Should().NotBeNull();
        payment.Status.Should().Be(TaxPaymentStatus.Cancelled);
        await _repo.Received(1).UpdateAsync(payment, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_NotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetWithDetailsAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new CancelTaxPaymentCommand(Guid.NewGuid(), "Erro"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}
