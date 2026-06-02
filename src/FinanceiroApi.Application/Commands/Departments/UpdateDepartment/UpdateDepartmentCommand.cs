using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;
using MediatR;

namespace FinanceiroApi.Application.Commands.Departments.UpdateDepartment;

public record UpdateDepartmentCommand(
    Guid Id,
    string Name,
    string CostCenter,
    string? Description) : IRequest<DepartmentResponse?>;

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentResponse?>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public UpdateDepartmentCommandHandler(
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

    public async Task<DepartmentResponse?> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
        {
            _notifications.AddNotification("Id", "Departamento não encontrado.");
            return null;
        }

        department.Update(request.Name, request.CostCenter, request.Description);

        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return _mapper.Map<DepartmentResponse>(department);
    }
}