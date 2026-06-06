using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.TaxPayments.CreateTaxPayment;

public record CreateTaxPaymentCommand(
    Guid TaxEntryId,
    Guid BankAccountId,
    decimal Amount,
    DateOnly PaymentDate,
    decimal Fine,
    decimal Interest,
    string? DarfNumber,
    string? ReceiptCode,
    string? Notes) : IRequest<TaxPaymentResponse>;

public class CreateTaxPaymentCommandHandler : IRequestHandler<CreateTaxPaymentCommand, TaxPaymentResponse>
{
    private readonly ITaxPaymentRepository _taxPaymentRepository;
    private readonly ITaxEntryRepository _taxEntryRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateTaxPaymentCommandHandler(
        ITaxPaymentRepository taxPaymentRepository,
        ITaxEntryRepository taxEntryRepository,
        IBankAccountRepository bankAccountRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _taxPaymentRepository = taxPaymentRepository;
        _taxEntryRepository = taxEntryRepository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<TaxPaymentResponse> Handle(CreateTaxPaymentCommand request, CancellationToken cancellationToken)
    {
        var taxEntry = await _taxEntryRepository.GetByIdAsync(request.TaxEntryId, cancellationToken);
        if (taxEntry is null)
        {
            _notifications.AddNotification("TaxEntryId", "Lançamento fiscal não encontrado.");
            return null!;
        }

        var bankAccountExists = await _bankAccountRepository.ExistsAsync(request.BankAccountId, cancellationToken);
        if (!bankAccountExists)
        {
            _notifications.AddNotification("BankAccountId", "Conta bancária não encontrada.");
            return null!;
        }

        var payment = TaxPayment.Create(
            request.TaxEntryId,
            request.BankAccountId,
            request.Amount,
            request.PaymentDate,
            request.Fine,
            request.Interest,
            request.DarfNumber,
            request.ReceiptCode,
            request.Notes);

        taxEntry.MarkAsPaid();

        await _taxPaymentRepository.AddAsync(payment, cancellationToken);
        await _taxEntryRepository.UpdateAsync(taxEntry, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var result = await _taxPaymentRepository.GetWithDetailsAsync(payment.Id, cancellationToken);
        return _mapper.Map<TaxPaymentResponse>(result!);
    }
}
