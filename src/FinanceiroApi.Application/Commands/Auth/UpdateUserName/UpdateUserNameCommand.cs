using FinanceiroApi.CrossCutting.Constants;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.Auth.UpdateUserName;

public record UpdateUserNameCommand(Guid UserId, string Name) : IRequest<bool>;

public class UpdateUserNameCommandHandler : IRequestHandler<UpdateUserNameCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationContext _notifications;

    public UpdateUserNameCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationContext notifications)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notifications = notifications;
    }

    public async Task<bool> Handle(UpdateUserNameCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            _notifications.AddNotification("UserId", "Usuário não encontrado.");
            return false;
        }

        if (user.Email.Equals(DemoAccount.Email, StringComparison.OrdinalIgnoreCase))
        {
            _notifications.AddNotification("UserId", "Esta é a conta demo do sistema e seu nome não pode ser alterado.");
            return false;
        }

        user.UpdateName(request.Name);

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return true;
    }
}
