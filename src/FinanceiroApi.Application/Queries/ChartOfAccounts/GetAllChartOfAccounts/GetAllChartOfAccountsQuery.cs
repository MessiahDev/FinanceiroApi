using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.ChartOfAccounts.GetAllChartOfAccounts;

public record GetAllChartOfAccountsQuery(
    bool? IsActive = null,
    AccountType? AccountType = null,
    bool OnlyRoots = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<ChartOfAccountSummaryResponse>>;

public class GetAllChartOfAccountsQueryHandler
    : IRequestHandler<GetAllChartOfAccountsQuery, PagedResult<ChartOfAccountSummaryResponse>>
{
    private readonly IChartOfAccountRepository _repository;
    private readonly IMapper _mapper;

    public GetAllChartOfAccountsQueryHandler(IChartOfAccountRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ChartOfAccountSummaryResponse>> Handle(
        GetAllChartOfAccountsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(
            request.IsActive, request.AccountType, request.OnlyRoots,
            request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<ChartOfAccountSummaryResponse>(
            _mapper.Map<IReadOnlyList<ChartOfAccountSummaryResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
