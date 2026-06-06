using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace FinanceiroApi.Application.Commands.Payroll.ProcessPayroll;

public record ProcessPayrollCommand(
    int Month,
    int Year,
    List<Guid> EmployeeIds) : IRequest<PayrollResponse>;

public class ProcessPayrollCommandHandler : IRequestHandler<ProcessPayrollCommand, PayrollResponse>
{
    private readonly IPayrollRepository _payrollRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public ProcessPayrollCommandHandler(
        IPayrollRepository payrollRepository,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _payrollRepository = payrollRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<PayrollResponse> Handle(ProcessPayrollCommand request, CancellationToken cancellationToken)
    {
        var alreadyProcessed = await _payrollRepository.ExistsForPeriodAsync(
            request.Year,
            request.Month,
            cancellationToken);

        if (alreadyProcessed)
        {
            _notifications.AddNotification("Period", $"Folha {request.Month:D2}/{request.Year} jÃ¡ foi processada.");
            return default!;
        }

        var payroll = Domain.Entities.Payroll.Create(request.Year, request.Month);

        foreach (var employeeId in request.EmployeeIds)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
            if (employee is null) continue;

            var gross = employee.BaseSalary;
            var inssAmount = FinanceiroApi.CrossCutting.Helpers.MoneyHelper.CalculateInss(gross.Amount);
            var irpfAmount = FinanceiroApi.CrossCutting.Helpers.MoneyHelper.CalculateIrrf(gross.Amount, inssAmount);
            var inss = new Domain.ValueObjects.Money(inssAmount);
            var irpf = new Domain.ValueObjects.Money(Math.Max(0, irpfAmount));
            var others = Domain.ValueObjects.Money.Zero;

            payroll.AddItem(employeeId, gross, inss, irpf, others);
        }

        if (!payroll.Items.Any())
        {
            _notifications.AddNotification("EmployeeIds", "Nenhum funcionÃ¡rio vÃ¡lido encontrado.");
            return default!;
        }

        payroll.Process();

        await _payrollRepository.AddAsync(payroll, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<PayrollResponse>(payroll);
    }
}

public class ProcessPayrollCommandValidator : AbstractValidator<ProcessPayrollCommand>
{
    public ProcessPayrollCommandValidator()
    {
        RuleFor(x => x.Month).InclusiveBetween(1, 12).WithMessage("MÃªs deve estar entre 1 e 12.");
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100).WithMessage("Ano invÃ¡lido.");
        RuleFor(x => x.EmployeeIds).NotEmpty().WithMessage("Informe ao menos um funcionÃ¡rio.");
    }
}
