using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.BankReconciliations.GetBankReconciliationById;

public record GetBankReconciliationByIdQuery(Guid Id) : IRequest<BankReconciliationResponse>;

public class GetBankReconciliationByIdQueryHandler : IRequestHandler<GetBankReconciliationByIdQuery, BankReconciliationResponse>
{
    private readonly IBankReconciliationRepository _reconciliationRepository;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public GetBankReconciliationByIdQueryHandler(
        IBankReconciliationRepository reconciliationRepository,
        IMapper mapper,
        INotificationContext notifications)
    {
        _reconciliationRepository = reconciliationRepository;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<BankReconciliationResponse> Handle(GetBankReconciliationByIdQuery request, CancellationToken cancellationToken)
    {
        var reconciliation = await _reconciliationRepository.GetWithItemsAsync(request.Id, cancellationToken);
        if (reconciliation is null)
        {
            _notifications.AddNotification("Id", "Conciliação bancária não encontrada.");
            return null!;
        }
        return _mapper.Map<BankReconciliationResponse>(reconciliation);
    }
}
