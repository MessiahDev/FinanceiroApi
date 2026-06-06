using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Queries.TaxPayments.GetTaxPaymentById;

public record GetTaxPaymentByIdQuery(Guid Id) : IRequest<TaxPaymentResponse>;

public class GetTaxPaymentByIdQueryHandler : IRequestHandler<GetTaxPaymentByIdQuery, TaxPaymentResponse>
{
    private readonly ITaxPaymentRepository _taxPaymentRepository;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public GetTaxPaymentByIdQueryHandler(
        ITaxPaymentRepository taxPaymentRepository,
        IMapper mapper,
        INotificationContext notifications)
    {
        _taxPaymentRepository = taxPaymentRepository;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<TaxPaymentResponse> Handle(GetTaxPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _taxPaymentRepository.GetWithDetailsAsync(request.Id, cancellationToken);
        if (payment is null)
        {
            _notifications.AddNotification("Id", "Pagamento fiscal não encontrado.");
            return null!;
        }
        return _mapper.Map<TaxPaymentResponse>(payment);
    }
}
