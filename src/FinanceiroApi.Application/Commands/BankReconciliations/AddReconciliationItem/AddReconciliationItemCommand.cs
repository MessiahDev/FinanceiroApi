using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.BankReconciliations.AddReconciliationItem;

public record AddReconciliationItemCommand(
    Guid ReconciliationId,
    Guid BankStatementEntryId,
    Guid? TransactionId,
    decimal Amount,
    ReconciliationItemStatus ItemStatus,
    string? Notes) : IRequest<BankReconciliationResponse>;

public class AddReconciliationItemCommandHandler : IRequestHandler<AddReconciliationItemCommand, BankReconciliationResponse>
{
    private readonly IBankReconciliationRepository _reconciliationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public AddReconciliationItemCommandHandler(
        IBankReconciliationRepository reconciliationRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _reconciliationRepository = reconciliationRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<BankReconciliationResponse> Handle(AddReconciliationItemCommand request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetWithItemsAsync(request.ReconciliationId, cancellationToken);
        if (reconciliation is null)
        {
            _notifications.AddNotification("ReconciliationId", "Conciliação bancária não encontrada.");
            return null!;
        }

        reconciliation.AddItem(
            request.BankStatementEntryId,
            request.TransactionId,
            request.Amount,
            request.ItemStatus,
            request.Notes);

        await _reconciliationRepository.UpdateAsync(reconciliation, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var result = await _reconciliationRepository.GetWithItemsAsync(reconciliation.Id, cancellationToken);
        return _mapper.Map<BankReconciliationResponse>(result!);
    }
}
