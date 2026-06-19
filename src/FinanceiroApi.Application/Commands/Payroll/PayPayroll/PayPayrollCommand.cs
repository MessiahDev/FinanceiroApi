using FluentValidation;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.ValueObjects;
using MediatR;
namespace FinanceiroApi.Application.Commands.Payroll.PayPayroll;

public record PayPayrollCommand(Guid PayrollId, Guid BankAccountId) : IRequest<bool>;
public class PayPayrollCommandHandler : IRequestHandler<PayPayrollCommand, bool>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;
    public PayPayrollCommandHandler(
        IPayrollRepository payrollRepository,
        IBankAccountRepository bankAccountRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _payrollRepository = payrollRepository;
        _bankAccountRepository = bankAccountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }
    public async Task<bool> Handle(PayPayrollCommand request, CancellationToken cancellationToken)
    {
        var payroll = await _payrollRepository.GetByIdAsync(request.PayrollId, cancellationToken);
        if (payroll is null)
        {
            _notifications.AddNotification("PayrollId", "Folha de pagamento não encontrada.");
            return false;
        }
        if (payroll.Status != PayrollStatus.Approved)
        {
            _notifications.AddNotification("Status", "Esta folha precisa estar aprovada para ser paga.");
            return false;
        }
        var bankAccount = await _bankAccountRepository.GetByIdAsync(request.BankAccountId, cancellationToken);
        if (bankAccount is null)
        {
            _notifications.AddNotification("BankAccountId", "Conta bancária não encontrada.");
            return false;
        }
        bankAccount.Debit(new Money(payroll.TotalNet.Amount), $"Pagamento de folha {payroll.Period.Start:MM/yyyy}");
        var transaction = Transaction.Create(
            payroll.TotalNet.Amount,
            TransactionType.Debit,
            TransactionCategory.Salary,
            $"Pagamento de folha de pagamento - {payroll.Period.Start:MM/yyyy}",
            DateOnly.FromDateTime(DateTime.UtcNow),
            employeeId: null,
            payrollId: payroll.Id,
            bankAccountId: request.BankAccountId);
        payroll.MarkAsPaid();
        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _bankAccountRepository.UpdateAsync(bankAccount, cancellationToken);
        await _payrollRepository.UpdateAsync(payroll, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return true;
    }
}
public class PayPayrollCommandValidator : AbstractValidator<PayPayrollCommand>
{
    public PayPayrollCommandValidator()
    {
        RuleFor(x => x.PayrollId).NotEmpty();
        RuleFor(x => x.BankAccountId).NotEmpty();
    }
}