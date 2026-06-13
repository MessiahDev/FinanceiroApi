using FluentValidation;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Payroll.ApprovePayroll;

public record ApprovePayrollCommand(Guid PayrollId) : IRequest<bool>;

public class ApprovePayrollCommandHandler : IRequestHandler<ApprovePayrollCommand, bool>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public ApprovePayrollCommandHandler(
        IPayrollRepository payrollRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _payrollRepository = payrollRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(ApprovePayrollCommand request, CancellationToken cancellationToken)
    {
        var payroll = await _payrollRepository.GetByIdAsync(request.PayrollId, cancellationToken);
        if (payroll is null)
        {
            _notifications.AddNotification("PayrollId", "Folha de pagamento não encontrada.");
            return false;
        }

        if (payroll.Status != Domain.Enums.PayrollStatus.Processing)
        {
            _notifications.AddNotification("Status", "Esta folha não pode ser aprovada no estado atual.");
            return false;
        }

        payroll.Approve();
        await _payrollRepository.UpdateAsync(payroll, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}

public class ApprovePayrollCommandValidator : AbstractValidator<ApprovePayrollCommand>
{
    public ApprovePayrollCommandValidator()
    {
        RuleFor(x => x.PayrollId).NotEmpty();
    }
}
