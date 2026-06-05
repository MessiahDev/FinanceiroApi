using MediatR;
using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Pagination;

namespace FinanceiroApi.Application.Queries.JournalEntries.GetJournalEntryById;

public record GetJournalEntryByIdQuery(Guid Id) : IRequest<JournalEntryResponse>;

public class GetJournalEntryByIdQueryHandler : IRequestHandler<GetJournalEntryByIdQuery, JournalEntryResponse>
{
    private readonly IJournalEntryRepository _repository;
    private readonly IMapper _mapper;

    public GetJournalEntryByIdQueryHandler(IJournalEntryRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<JournalEntryResponse> Handle(
        GetJournalEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entry = await _repository.GetWithLinesAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Lançamento '{request.Id}' não encontrado.");

        return _mapper.Map<JournalEntryResponse>(entry);
    }
}
