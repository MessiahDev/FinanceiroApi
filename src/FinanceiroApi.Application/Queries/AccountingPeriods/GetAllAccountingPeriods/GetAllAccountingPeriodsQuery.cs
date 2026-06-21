using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.AccountingPeriods.GetAllAccountingPeriods;

public record GetAllAccountingPeriodsQuery(int? Year = null, int PageNumber = 1, int PageSize = 20)
    : IRequest<PagedResult<AccountingPeriodResponse>>;

public class GetAllAccountingPeriodsQueryHandler
    : IRequestHandler<GetAllAccountingPeriodsQuery, PagedResult<AccountingPeriodResponse>>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IMapper _mapper;

    public GetAllAccountingPeriodsQueryHandler(IAccountingPeriodRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<AccountingPeriodResponse>> Handle(
        GetAllAccountingPeriodsQuery request, CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(request.Year, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<AccountingPeriodResponse>(
            _mapper.Map<IReadOnlyList<AccountingPeriodResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
