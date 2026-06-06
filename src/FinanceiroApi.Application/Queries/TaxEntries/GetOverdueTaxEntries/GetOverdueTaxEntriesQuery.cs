using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.TaxEntries.GetOverdueTaxEntries;

public record GetOverdueTaxEntriesQuery : IRequest<IReadOnlyList<TaxEntrySummaryResponse>>;

public class GetOverdueTaxEntriesQueryHandler : IRequestHandler<GetOverdueTaxEntriesQuery, IReadOnlyList<TaxEntrySummaryResponse>>
{
    private readonly ITaxEntryRepository _taxEntryRepository;
    private readonly IMapper _mapper;

    public GetOverdueTaxEntriesQueryHandler(ITaxEntryRepository taxEntryRepository, IMapper mapper)
    {
        _taxEntryRepository = taxEntryRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TaxEntrySummaryResponse>> Handle(GetOverdueTaxEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _taxEntryRepository.GetOverdueAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TaxEntrySummaryResponse>>(entries);
    }
}
