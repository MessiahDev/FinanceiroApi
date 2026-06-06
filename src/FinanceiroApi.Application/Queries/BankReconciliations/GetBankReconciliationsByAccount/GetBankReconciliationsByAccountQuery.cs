using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.BankReconciliations.GetBankReconciliationsByAccount;

public record GetBankReconciliationsByAccountQuery(
    Guid? BankAccountId,
    ReconciliationStatus? Status) : IRequest<IReadOnlyList<BankReconciliationSummaryResponse>>;

public class GetBankReconciliationsByAccountQueryHandler
    : IRequestHandler<GetBankReconciliationsByAccountQuery, IReadOnlyList<BankReconciliationSummaryResponse>>
{
    private readonly IBankReconciliationRepository _reconciliationRepository;
    private readonly IMapper _mapper;

    public GetBankReconciliationsByAccountQueryHandler(IBankReconciliationRepository reconciliationRepository, IMapper mapper)
    {
        _reconciliationRepository = reconciliationRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<BankReconciliationSummaryResponse>> Handle(
        GetBankReconciliationsByAccountQuery request,
        CancellationToken cancellationToken)
    {
        var reconciliations = request.BankAccountId.HasValue
            ? await _reconciliationRepository.GetByBankAccountAsync(request.BankAccountId.Value, cancellationToken)
            : request.Status.HasValue
                ? await _reconciliationRepository.GetByStatusAsync(request.Status.Value, cancellationToken)
                : await _reconciliationRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<BankReconciliationSummaryResponse>>(reconciliations);
    }
}
