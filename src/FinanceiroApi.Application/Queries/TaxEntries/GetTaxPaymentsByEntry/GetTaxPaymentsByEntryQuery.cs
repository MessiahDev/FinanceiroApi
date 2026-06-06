using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.TaxPayments.GetTaxPaymentsByEntry;

public record GetTaxPaymentsByEntryQuery(Guid TaxEntryId) : IRequest<IReadOnlyList<TaxPaymentResponse>>;

public class GetTaxPaymentsByEntryQueryHandler : IRequestHandler<GetTaxPaymentsByEntryQuery, IReadOnlyList<TaxPaymentResponse>>
{
    private readonly ITaxPaymentRepository _taxPaymentRepository;
    private readonly IMapper _mapper;

    public GetTaxPaymentsByEntryQueryHandler(ITaxPaymentRepository taxPaymentRepository, IMapper mapper)
    {
        _taxPaymentRepository = taxPaymentRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TaxPaymentResponse>> Handle(GetTaxPaymentsByEntryQuery request, CancellationToken cancellationToken)
    {
        var payments = await _taxPaymentRepository.GetByTaxEntryAsync(request.TaxEntryId, cancellationToken);
        return _mapper.Map<IReadOnlyList<TaxPaymentResponse>>(payments);
    }
}
