using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.JournalEntries.GetJournalEntriesByPeriod;

public record GetJournalEntriesByPeriodQuery(
    Guid AccountingPeriodId,
    JournalEntryStatus? Status = null,
    JournalEntryType? EntryType = null
) : IRequest<IEnumerable<JournalEntrySummaryResponse>>;

public class GetJournalEntriesByPeriodQueryHandler
    : IRequestHandler<GetJournalEntriesByPeriodQuery, IEnumerable<JournalEntrySummaryResponse>>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IMapper _mapper;

    public GetJournalEntriesByPeriodQueryHandler(IJournalEntryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<JournalEntrySummaryResponse>> Handle(
        GetJournalEntriesByPeriodQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.GetByPeriodAsync(request.AccountingPeriodId, cancellationToken);

        if (request.Status.HasValue)
            entries = entries.Where(e => e.Status == request.Status.Value);

        if (request.EntryType.HasValue)
            entries = entries.Where(e => e.EntryType == request.EntryType.Value);

        return _mapper.Map<IEnumerable<JournalEntrySummaryResponse>>(entries);
    }
}
