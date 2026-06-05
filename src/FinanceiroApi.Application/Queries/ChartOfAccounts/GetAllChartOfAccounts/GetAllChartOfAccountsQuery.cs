using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.ChartOfAccounts.GetAllChartOfAccounts;

public record GetAllChartOfAccountsQuery(
    bool? IsActive = null,
    AccountType? AccountType = null,
    bool OnlyRoots = false
) : IRequest<IEnumerable<ChartOfAccountSummaryResponse>>;

public class GetAllChartOfAccountsQueryHandler
    : IRequestHandler<GetAllChartOfAccountsQuery, IEnumerable<ChartOfAccountSummaryResponse>>
{
    private readonly IChartOfAccountRepository _repository;
    private readonly IMapper _mapper;

    public GetAllChartOfAccountsQueryHandler(IChartOfAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ChartOfAccountSummaryResponse>> Handle(
        GetAllChartOfAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = request.OnlyRoots
            ? await _repository.GetRootAccountsAsync(cancellationToken)
            : request.AccountType.HasValue
                ? await _repository.GetByTypeAsync(request.AccountType.Value, cancellationToken)
                : await _repository.GetActiveAccountsAsync(cancellationToken);

        if (request.IsActive.HasValue)
            accounts = accounts.Where(a => a.IsActive == request.IsActive.Value);

        return _mapper.Map<IEnumerable<ChartOfAccountSummaryResponse>>(accounts);
    }
}
