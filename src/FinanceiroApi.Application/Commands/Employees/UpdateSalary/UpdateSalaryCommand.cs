using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentValidation;
using MediatR;

namespace FinanceiroApi.Application.Commands.Employees.UpdateSalary;

public record UpdateSalaryCommand(
    Guid EmployeeId,
    decimal NewSalary,
    string Reason) : IRequest<EmployeeResponse>;

public class UpdateSalaryCommandHandler : IRequestHandler<UpdateSalaryCommand, EmployeeResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public UpdateSalaryCommandHandler(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<EmployeeResponse> Handle(UpdateSalaryCommand request, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee is null)
        {
            _notifications.AddNotification("EmployeeId", "Funcionário não encontrado.");
            return null!;
        }

        employee.UpdateSalary(request.NewSalary);

        await _employeeRepository.UpdateAsync(employee, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<EmployeeResponse>(employee);
    }
}

public class UpdateSalaryCommandValidator : AbstractValidator<UpdateSalaryCommand>
{
    public UpdateSalaryCommandValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.NewSalary).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}
