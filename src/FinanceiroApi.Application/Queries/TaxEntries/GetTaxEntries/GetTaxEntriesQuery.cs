using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
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
    DateOnly? DueDateTo) : IRequest<IReadOnlyList<TaxEntrySummaryResponse>>;

public class GetTaxEntriesQueryHandler : IRequestHandler<GetTaxEntriesQuery, IReadOnlyList<TaxEntrySummaryResponse>>
{
    private readonly ITaxEntryRepository _taxEntryRepository;
    private readonly IMapper _mapper;

    public GetTaxEntriesQueryHandler(ITaxEntryRepository taxEntryRepository, IMapper mapper)
    {
        _taxEntryRepository = taxEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TaxEntrySummaryResponse>> Handle(GetTaxEntriesQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Domain.Entities.TaxEntry> entries;

        if (request.TaxType.HasValue)
            entries = await _taxEntryRepository.GetByTaxTypeAsync(request.TaxType.Value, cancellationToken);
        else if (request.Status.HasValue)
            entries = await _taxEntryRepository.GetByStatusAsync(request.Status.Value, cancellationToken);
        else if (request.CompetenceYear.HasValue && request.CompetenceMonth.HasValue)
            entries = await _taxEntryRepository.GetByCompetenceAsync(request.CompetenceYear.Value, request.CompetenceMonth.Value, cancellationToken);
        else if (request.DueDateFrom.HasValue && request.DueDateTo.HasValue)
            entries = await _taxEntryRepository.GetByDueDateRangeAsync(request.DueDateFrom.Value, request.DueDateTo.Value, cancellationToken);
        else
            entries = await _taxEntryRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<TaxEntrySummaryResponse>>(entries);
    }
}
