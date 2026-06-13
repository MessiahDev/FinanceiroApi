using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Transactions.CreateTransaction;

public record CreateTransactionCommand(
    string Description,
    decimal Amount,
    string Type,
    string Category,
    Guid? EmployeeId,
    Guid? PayrollId,
    string? ReferenceNumber,
    DateOnly? TransactionDate) : IRequest<TransactionResponse>;

public class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateTransactionCommandHandler(
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<TransactionResponse> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TransactionType>(request.Type, true, out var type))
        {
            _notifications.AddNotification("Type", "Tipo de transação inválido. Use 'Debit' ou 'Credit'.");
            return null!;
        }

        if (!Enum.TryParse<TransactionCategory>(request.Category, true, out var category))
        {
            _notifications.AddNotification("Category", "Categoria de transação inválida.");
            return null!;
        }

        var transaction = Transaction.Create(
            request.Amount,
            type,
            category,
            request.Description,
            request.TransactionDate,
            request.EmployeeId,
            request.PayrollId,
            request.ReferenceNumber);

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<TransactionResponse>(transaction);
    }
}