using FinanceiroApi.CrossCutting.Constants;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Users.ChangeUserRole;

public record ChangeUserRoleCommand(Guid TargetUserId, UserRole NewRole, Guid ChangedByUserId) : IRequest<bool>;

public class ChangeUserRoleCommandHandler : IRequestHandler<ChangeUserRoleCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public ChangeUserRoleCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.TargetUserId, cancellationToken);
        if (user is null)
        {
            _notifications.AddNotification("TargetUserId", "Usuário não encontrado.");
            return false;
        }

        if (user.Email.Equals(DemoAccount.Email, StringComparison.OrdinalIgnoreCase))
        {
            _notifications.AddNotification("TargetUserId", "Esta é a conta demo do sistema e seu nível de acesso não pode ser alterado.");
            return false;
        }

        user.ChangeRole(request.NewRole, request.ChangedByUserId);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
