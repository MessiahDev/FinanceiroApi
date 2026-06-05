using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.AccountsReceivable.GetOpenReceivables;

public record GetOpenReceivablesQuery(Guid? CustomerId = null) : IRequest<IReadOnlyList<AccountReceivableResponse>>;

public class GetOpenReceivablesQueryHandler : IRequestHandler<GetOpenReceivablesQuery, IReadOnlyList<AccountReceivableResponse>>
{
    private readonly IAccountReceivableRepository _accountReceivableRepository;
    private readonly IMapper _mapper;

    public GetOpenReceivablesQueryHandler(IAccountReceivableRepository accountReceivableRepository, IMapper mapper)
    {
        _accountReceivableRepository = accountReceivableRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<AccountReceivableResponse>> Handle(
        GetOpenReceivablesQuery request,
        CancellationToken cancellationToken)
    {
        var receivables = request.CustomerId.HasValue
            ? await _accountReceivableRepository.GetByCustomerAsync(request.CustomerId.Value, cancellationToken)
            : await _accountReceivableRepository.GetOpenAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<AccountReceivableResponse>>(receivables);
    }
}
