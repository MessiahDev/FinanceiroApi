using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Transactions.ConfirmTransaction;

public record ConfirmTransactionCommand(Guid Id) : IRequest<TransactionResponse?>;

public class ConfirmTransactionCommandHandler : IRequestHandler<ConfirmTransactionCommand, TransactionResponse?>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public ConfirmTransactionCommandHandler(
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

    public async Task<TransactionResponse?> Handle(ConfirmTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null)
        {
            _notifications.AddNotification("Id", "Transação não encontrada.");
            return null;
        }

        transaction.Confirm();

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<TransactionResponse>(transaction);
    }
}