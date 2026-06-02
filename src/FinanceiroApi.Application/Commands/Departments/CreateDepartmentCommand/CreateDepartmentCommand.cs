using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Departments.CreateDepartment;

public record CreateDepartmentCommand(
    string Name,
    string CostCenter,
    string? Description) : IRequest<DepartmentResponse>;

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentResponse>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public CreateDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<DepartmentResponse> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var exists = await _departmentRepository.ExistsByNameAsync(request.Name, cancellationToken);
        if (exists)
        {
            _notifications.AddNotification("Name", "Já existe um departamento com este nome.");
            return null!;
        }

        var department = Department.Create(request.Name, request.CostCenter, request.Description);

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<DepartmentResponse>(department);
    }
}