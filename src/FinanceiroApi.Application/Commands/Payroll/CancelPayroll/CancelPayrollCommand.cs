using FluentValidation;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Payroll.CancelPayroll;

public record CancelPayrollCommand(Guid PayrollId, string Reason) : IRequest<bool>;

public class CancelPayrollCommandHandler : IRequestHandler<CancelPayrollCommand, bool>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public CancelPayrollCommandHandler(
        IPayrollRepository payrollRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _payrollRepository = payrollRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(CancelPayrollCommand request, CancellationToken cancellationToken)
    {
        var payroll = await _payrollRepository.GetByIdAsync(request.PayrollId, cancellationToken);
        if (payroll is null)
        {
            _notifications.AddNotification("PayrollId", "Folha de pagamento não encontrada.");
            return false;
        }

        if (payroll.Status == Domain.Enums.PayrollStatus.Paid)
        {
            _notifications.AddNotification("Status", "Esta folha não pode ser cancelada no estado atual.");
            return false;
        }

        payroll.Cancel(request.Reason);
        await _payrollRepository.UpdateAsync(payroll, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}

public class CancelPayrollCommandValidator : AbstractValidator<CancelPayrollCommand>
{
    public CancelPayrollCommandValidator()
    {
        RuleFor(x => x.PayrollId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
