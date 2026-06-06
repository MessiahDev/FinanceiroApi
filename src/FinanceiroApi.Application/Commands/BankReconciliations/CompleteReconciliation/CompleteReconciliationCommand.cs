using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.BankReconciliations.CompleteReconciliation;

public record CompleteReconciliationCommand(Guid Id, Guid CompletedBy) : IRequest<BankReconciliationResponse>;

public class CompleteReconciliationCommandHandler : IRequestHandler<CompleteReconciliationCommand, BankReconciliationResponse>
{
    private readonly IBankReconciliationRepository _reconciliationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CompleteReconciliationCommandHandler(
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

    public async Task<BankReconciliationResponse> Handle(CompleteReconciliationCommand request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetWithItemsAsync(request.Id, cancellationToken);
        if (reconciliation is null)
        {
            _notifications.AddNotification("Id", "Conciliação bancária não encontrada.");
            return null!;
        }

        reconciliation.Complete(request.CompletedBy);

        await _reconciliationRepository.UpdateAsync(reconciliation, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<BankReconciliationResponse>(reconciliation);
    }
}
