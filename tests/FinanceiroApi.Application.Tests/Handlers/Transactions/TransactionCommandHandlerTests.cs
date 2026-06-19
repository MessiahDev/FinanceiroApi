using AutoMapper;
using FinanceiroApi.Application.Commands.Transactions.CancelTransaction;
using FinanceiroApi.Application.Commands.Transactions.ConfirmTransaction;
using FinanceiroApi.Application.Commands.Transactions.CreateTransaction;
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

namespace FinanceiroApi.Application.Tests.Handlers.Transactions;

public class CreateTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _repo = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CreateTransactionCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static TransactionResponse MakeResponse(Transaction t) => new(
        t.Id, t.Description, t.Amount.Amount, "BRL",
        t.Type.ToString(), t.Category.ToString(), t.Status.ToString(),
        t.TransactionDate, t.EmployeeId, t.PayrollId, t.BankAccountId, t.ReferenceNumber, DateTime.UtcNow);

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTransactionAndReturnResponse()
    {
        var cmd = new CreateTransactionCommand(
            "Salário", 5000m, "Credit", "Salary", null, null,
            "REF-001", DateOnly.FromDateTime(DateTime.UtcNow));

        _mapper.Map<TransactionResponse>(Arg.Any<Transaction>())
               .Returns(c => MakeResponse(c.Arg<Transaction>()));

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result.Description.Should().Be("Salário");
        await _repo.Received(1).AddAsync(Arg.Any<Transaction>(), default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_InvalidType_ShouldNotifyAndReturnNull()
    {
        var cmd = new CreateTransactionCommand(
            "Teste", 100m, "Invalido", "Salary", null, null,
            "REF-002", null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Type", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_InvalidCategory_ShouldNotifyAndReturnNull()
    {
        var cmd = new CreateTransactionCommand(
            "Teste", 100m, "Credit", "CategoriaInvalida", null, null,
            "REF-003", null);

        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Category", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }
}

public class ConfirmTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _repo = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private ConfirmTransactionCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static Transaction MakePendingTransaction() =>
        Transaction.Create(1000m, TransactionType.Credit, TransactionCategory.Salary, "Salário");

    private static TransactionResponse MakeResponse(Transaction t) => new(
        t.Id, t.Description, t.Amount.Amount, "BRL",
        t.Type.ToString(), t.Category.ToString(), t.Status.ToString(),
        t.TransactionDate, t.EmployeeId, t.PayrollId, t.BankAccountId, t.ReferenceNumber, DateTime.UtcNow);

    [Fact]
    public async Task Handle_PendingTransaction_ShouldConfirmAndReturnResponse()
    {
        var transaction = MakePendingTransaction();
        _repo.GetByIdAsync(transaction.Id, default).Returns(transaction);
        _mapper.Map<TransactionResponse>(Arg.Any<Transaction>())
               .Returns(c => MakeResponse(c.Arg<Transaction>()));

        var result = await CreateHandler().Handle(new ConfirmTransactionCommand(transaction.Id), default);

        result.Should().NotBeNull();
        transaction.Status.Should().Be(TransactionStatus.Confirmed);
        await _repo.Received(1).UpdateAsync(transaction, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_TransactionNotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(new ConfirmTransactionCommand(Guid.NewGuid()), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_AlreadyConfirmedTransaction_ShouldThrowDomainException()
    {
        var transaction = MakePendingTransaction();
        transaction.Confirm();
        _repo.GetByIdAsync(transaction.Id, default).Returns(transaction);

        var act = () => CreateHandler().Handle(new ConfirmTransactionCommand(transaction.Id), default);

        await act.Should().ThrowAsync<Exception>();
    }
}

public class CancelTransactionCommandHandlerTests
{
    private readonly ITransactionRepository _repo = Substitute.For<ITransactionRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IMapper _mapper = Substitute.For<IMapper>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();

    private CancelTransactionCommandHandler CreateHandler() => new(_repo, _uow, _mapper, _notif);

    private static Transaction MakePendingTransaction() =>
        Transaction.Create(1000m, TransactionType.Debit, TransactionCategory.Tax, "Imposto");

    private static TransactionResponse MakeResponse(Transaction t) => new(
        t.Id, t.Description, t.Amount.Amount, "BRL",
        t.Type.ToString(), t.Category.ToString(), t.Status.ToString(),
        t.TransactionDate, t.EmployeeId, t.PayrollId, t.BankAccountId, t.ReferenceNumber, DateTime.UtcNow);

    [Fact]
    public async Task Handle_PendingTransaction_ShouldCancelAndReturnResponse()
    {
        var transaction = MakePendingTransaction();
        _repo.GetByIdAsync(transaction.Id, default).Returns(transaction);
        _mapper.Map<TransactionResponse>(Arg.Any<Transaction>())
               .Returns(c => MakeResponse(c.Arg<Transaction>()));

        var result = await CreateHandler().Handle(
            new CancelTransactionCommand(transaction.Id, "Erro no lançamento"), default);

        result.Should().NotBeNull();
        transaction.Status.Should().Be(TransactionStatus.Cancelled);
        await _repo.Received(1).UpdateAsync(transaction, default);
        await _uow.Received(1).CommitAsync(default);
    }

    [Fact]
    public async Task Handle_TransactionNotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetByIdAsync(Arg.Any<Guid>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(
            new CancelTransactionCommand(Guid.NewGuid(), "Motivo"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Id", Arg.Any<string>());
        await _uow.DidNotReceive().CommitAsync(default);
    }

    [Fact]
    public async Task Handle_ConfirmedTransaction_ShouldThrowDomainException()
    {
        var transaction = MakePendingTransaction();
        transaction.Confirm();
        _repo.GetByIdAsync(transaction.Id, default).Returns(transaction);

        var act = () => CreateHandler().Handle(
            new CancelTransactionCommand(transaction.Id, "Tentativa de cancelar confirmada"), default);

        await act.Should().ThrowAsync<Exception>();
    }
}
