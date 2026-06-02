using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.ValueObjects;
using MediatR;

namespace FinanceiroApi.Application.Commands.Employees.CreateEmployee;

public record CreateEmployeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string Cpf,
    string Position,
    Guid DepartmentId,
    decimal BaseSalary,
    ContractType ContractType) : IRequest<EmployeeResponse>;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeResponse>
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateEmployeeCommandHandler(
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

    public async Task<EmployeeResponse> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
        {
            _notifications.AddNotification("DepartmentId", "Departamento não encontrado.");
            return null!;
        }

        var cpfExists = await _employeeRepository.ExistsByCpfAsync(request.Cpf, cancellationToken);
        if (cpfExists)
        {
            _notifications.AddNotification("Cpf", "CPF já cadastrado.");
            return null!;
        }

        var employee = Employee.Create(
            request.FirstName,
            request.LastName,
            request.Cpf,
            request.Email,
            request.BaseSalary,
            request.ContractType,
            request.DepartmentId,
            request.Position);

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<EmployeeResponse>(employee);
    }
}