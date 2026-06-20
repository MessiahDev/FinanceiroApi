using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Users.DeactivateUser;

public record DeactivateUserCommand(Guid TargetUserId, Guid ChangedByUserId) : IRequest<bool>;

public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public DeactivateUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.TargetUserId, cancellationToken);
        if (user is null)
        {
            _notifications.AddNotification("TargetUserId", "Usuário não encontrado.");
            return false;
        }

        if (user.Id == request.ChangedByUserId)
        {
            _notifications.AddNotification("TargetUserId", "Você não pode desativar sua própria conta.");
            return false;
        }

        user.Deactivate(request.ChangedByUserId);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
