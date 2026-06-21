using FinanceiroApi.CrossCutting.Constants;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Auth.ChangePassword;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public ChangePasswordCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            _notifications.AddNotification("UserId", "Usuário não encontrado.");
            return false;
        }

        if (user.Email.Equals(DemoAccount.Email, StringComparison.OrdinalIgnoreCase))
        {
            _notifications.AddNotification("UserId", "Esta é a conta demo do sistema e sua senha não pode ser alterada.");
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            _notifications.AddNotification("CurrentPassword", "Senha atual incorreta.");
            return false;
        }

        var newHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePasswordHash(newHash);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
