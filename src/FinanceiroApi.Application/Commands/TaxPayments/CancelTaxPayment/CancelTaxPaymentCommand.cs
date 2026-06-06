using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.TaxPayments.CancelTaxPayment;

public record CancelTaxPaymentCommand(Guid Id, string Reason) : IRequest<TaxPaymentResponse>;

public class CancelTaxPaymentCommandHandler : IRequestHandler<CancelTaxPaymentCommand, TaxPaymentResponse>
{
    private readonly ITaxPaymentRepository _taxPaymentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CancelTaxPaymentCommandHandler(
        ITaxPaymentRepository taxPaymentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _taxPaymentRepository = taxPaymentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<TaxPaymentResponse> Handle(CancelTaxPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _taxPaymentRepository.GetWithDetailsAsync(request.Id, cancellationToken);
        if (payment is null)
        {
            _notifications.AddNotification("Id", "Pagamento fiscal não encontrado.");
            return null!;
        }

        payment.Cancel(request.Reason);

        await _taxPaymentRepository.UpdateAsync(payment, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<TaxPaymentResponse>(payment);
    }
}