using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.TaxEntries.GetTaxEntryById;

public record GetTaxEntryByIdQuery(Guid Id) : IRequest<TaxEntryResponse>;

public class GetTaxEntryByIdQueryHandler : IRequestHandler<GetTaxEntryByIdQuery, TaxEntryResponse>
{
    private readonly ITaxEntryRepository _taxEntryRepository;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public GetTaxEntryByIdQueryHandler(
        ITaxEntryRepository taxEntryRepository,
        IMapper mapper,
        INotificationContext notifications)
    {
        _taxEntryRepository = taxEntryRepository;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<TaxEntryResponse> Handle(GetTaxEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var entry = await _taxEntryRepository.GetWithPaymentsAsync(request.Id, cancellationToken);
        if (entry is null)
        {
            _notifications.AddNotification("Id", "Lançamento fiscal não encontrado.");
            return null!;
        }
        return _mapper.Map<TaxEntryResponse>(entry);
    }
}
