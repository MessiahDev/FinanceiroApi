using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.AccountsPayable.PayAccountPayable;

public record PayAccountPayableCommand(
    Guid Id,
    decimal Amount,
    DateOnly PaymentDate,
    Guid BankAccountId) : IRequest<AccountPayableResponse?>;

public class PayAccountPayableCommandHandler : IRequestHandler<PayAccountPayableCommand, AccountPayableResponse?>
{
    private readonly IAccountPayableRepository _accountPayableRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public PayAccountPayableCommandHandler(
        IAccountPayableRepository accountPayableRepository,
        IBankAccountRepository bankAccountRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _accountPayableRepository = accountPayableRepository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<AccountPayableResponse?> Handle(PayAccountPayableCommand request, CancellationToken cancellationToken)
    {
        var payable = await _accountPayableRepository.GetByIdAsync(request.Id, cancellationToken);
        if (payable is null)
        {
            _notifications.AddNotification("Id", "Conta a pagar não encontrada.");
            return null;
        }

        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
        if (bankAccount is null)
        {
            _notifications.AddNotification("BankAccountId", "Conta bancária não encontrada.");
            return null;
        }

        payable.RegisterPayment(request.Amount, request.PaymentDate, request.BankAccountId);
        bankAccount.Debit(new FinanceiroApi.Domain.ValueObjects.Money(request.Amount), payable.Description);

        await _accountPayableRepository.UpdateAsync(payable, cancellationToken);
        await _bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<AccountPayableResponse>(payable);
    }
}
