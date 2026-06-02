using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;
using MediatR;

namespace FinanceiroApi.Application.Commands.Departments.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : IRequest<bool>;

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, bool>
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public DeleteDepartmentCommandHandler(
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _departmentRepository.GetByIdAsync(request.Id, cancellationToken);
        if (department is null)
        {
            _notifications.AddNotification("Id", "Departamento não encontrado.");
            return false;
        }

        await _departmentRepository.DeleteAsync(department, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}