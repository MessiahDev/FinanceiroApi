using MediatR;
using AutoMapper;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Commands.AccountsReceivable.ReceivePayment;

public record ReceivePaymentCommand(
    Guid Id,
    decimal Amount,
    DateOnly ReceiptDate,
    Guid BankAccountId) : IRequest<AccountReceivableResponse?>;

public class ReceivePaymentCommandHandler : IRequestHandler<ReceivePaymentCommand, AccountReceivableResponse?>
{
    private readonly IAccountReceivableRepository _accountReceivableRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public ReceivePaymentCommandHandler(
        IAccountReceivableRepository accountReceivableRepository,
        IBankAccountRepository bankAccountRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _accountReceivableRepository = accountReceivableRepository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<AccountReceivableResponse?> Handle(ReceivePaymentCommand request, CancellationToken cancellationToken)
    {
        var receivable = await _accountReceivableRepository.GetByIdAsync(request.Id, cancellationToken);
        if (receivable is null)
        {
            _notifications.AddNotification("Id", "Conta a receber não encontrada.");
            return null;
        }

        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
        if (bankAccount is null)
        {
            _notifications.AddNotification("BankAccountId", "Conta bancária não encontrada.");
            return null;
        }

        receivable.RegisterReceipt(request.Amount, request.ReceiptDate, request.BankAccountId);
        bankAccount.Credit(new FinanceiroApi.Domain.ValueObjects.Money(request.Amount), receivable.Description);

        await _accountReceivableRepository.UpdateAsync(receivable, cancellationToken);
        await _bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<AccountReceivableResponse>(receivable);
    }
}
