using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.BankAccounts.TransferBetweenAccounts;

public record TransferBetweenAccountsCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Description) : IRequest<bool>;

public class TransferBetweenAccountsCommandHandler : IRequestHandler<TransferBetweenAccountsCommand, bool>
{
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;
    public TransferBetweenAccountsCommandHandler(
        IBankAccountRepository bankAccountRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _bankAccountRepository = bankAccountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }
    public async Task<bool> Handle(TransferBetweenAccountsCommand request, CancellationToken cancellationToken)
    {
        if (request.SourceAccountId == request.DestinationAccountId)
        {
            _notifications.AddNotification("AccountId", "Conta de origem e destino não podem ser iguais.");
            return false;
        }
        var source = await _bankAccountRepository.GetByIdAsync(request.SourceAccountId, cancellationToken);
        if (source is null)
        {
            _notifications.AddNotification("SourceAccountId", "Conta de origem não encontrada.");
            return false;
        }
        var destination = await _bankAccountRepository.GetByIdAsync(request.DestinationAccountId, cancellationToken);
        if (destination is null)
        {
            _notifications.AddNotification("DestinationAccountId", "Conta de destino não encontrada.");
            return false;
        }
        var money = new FinanceiroApi.Domain.ValueObjects.Money(request.Amount);
        source.Debit(money, $"Transferência: {request.Description}");
        destination.Credit(money, $"Transferência recebida: {request.Description}");

        var transactionDate = DateOnly.FromDateTime(DateTime.UtcNow);

        var outTransaction = Transaction.Create(
            request.Amount,
            TransactionType.Debit,
            TransactionCategory.Other,
            $"Transferência enviada: {request.Description}",
            transactionDate,
            referenceNumber: $"TRANSFER-{source.Id}->{destination.Id}");

        var inTransaction = Transaction.Create(
            request.Amount,
            TransactionType.Credit,
            TransactionCategory.Other,
            $"Transferência recebida: {request.Description}",
            transactionDate,
            referenceNumber: $"TRANSFER-{source.Id}->{destination.Id}");

        await _bankAccountRepository.UpdateAsync(source, cancellationToken);
        await _bankAccountRepository.UpdateAsync(destination, cancellationToken);
        await _transactionRepository.AddAsync(outTransaction, cancellationToken);
        await _transactionRepository.AddAsync(inTransaction, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return true;
    }
}