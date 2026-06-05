using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Transactions.CancelTransaction;

public record CancelTransactionCommand(Guid Id, string Reason) : IRequest<TransactionResponse?>;

public class CancelTransactionCommandHandler : IRequestHandler<CancelTransactionCommand, TransactionResponse?>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CancelTransactionCommandHandler(
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

    public async Task<TransactionResponse?> Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await _transactionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (transaction is null)
        {
            _notifications.AddNotification("Id", "Transação não encontrada.");
            return null;
        }

        transaction.Cancel(request.Reason);

        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<TransactionResponse>(transaction);
    }
}