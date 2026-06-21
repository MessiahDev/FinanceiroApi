using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;
namespace FinanceiroApi.Application.Queries.TaxEntries.GetTaxEntries;

public record GetTaxEntriesQuery(
    TaxType? TaxType,
    TaxEntryStatus? Status,
    int? CompetenceYear,
    int? CompetenceMonth,
    DateOnly? DueDateFrom,
    DateOnly? DueDateTo,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<PagedResult<TaxEntrySummaryResponse>>;
public class GetTaxEntriesQueryHandler : IRequestHandler<GetTaxEntriesQuery, PagedResult<TaxEntrySummaryResponse>>
{
    private readonly ITaxEntryRepository _taxEntryRepository;
    private readonly IMapper _mapper;
    public GetTaxEntriesQueryHandler(ITaxEntryRepository taxEntryRepository, IMapper mapper)
    {
        _taxEntryRepository = taxEntryRepository;
        _mapper = mapper;
    }
    public async Task<PagedResult<TaxEntrySummaryResponse>> Handle(GetTaxEntriesQuery request, CancellationToken cancellationToken)
    {
        var result = await _taxEntryRepository.GetPagedAsync(
            request.TaxType, request.Status, request.CompetenceYear, request.CompetenceMonth,
            request.DueDateFrom, request.DueDateTo, request.PageNumber, request.PageSize, cancellationToken);

        return new PagedResult<TaxEntrySummaryResponse>(
            _mapper.Map<IReadOnlyList<TaxEntrySummaryResponse>>(result.Items),
            result.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
