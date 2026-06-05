using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.AccountingPeriods.GetAllAccountingPeriods;

public record GetAllAccountingPeriodsQuery(int? Year = null) : IRequest<IEnumerable<AccountingPeriodResponse>>;

public class GetAllAccountingPeriodsQueryHandler
    : IRequestHandler<GetAllAccountingPeriodsQuery, IEnumerable<AccountingPeriodResponse>>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IMapper _mapper;

    public GetAllAccountingPeriodsQueryHandler(IAccountingPeriodRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AccountingPeriodResponse>> Handle(
        GetAllAccountingPeriodsQuery request, CancellationToken cancellationToken)
    {
        var periods = request.Year.HasValue
            ? await _repository.GetByYearAsync(request.Year.Value, cancellationToken)
            : await _repository.GetOpenPeriodsAsync(cancellationToken);

        return _mapper.Map<IEnumerable<AccountingPeriodResponse>>(periods);
    }
}
