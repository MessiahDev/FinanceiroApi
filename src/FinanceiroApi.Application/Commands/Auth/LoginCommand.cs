using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Security;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinanceiroApi.Application.Commands.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponse?>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse?>
{
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;
    private readonly INotificationContext _notifications;
    private readonly JwtSettings _jwtSettings;

    public LoginCommandHandler(
        IUserRepository userRepository,
        TokenService tokenService,
        INotificationContext notifications,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _notifications = notifications;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<AuthResponse?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), cancellationToken);

        if (user is null || !user.IsActive || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _notifications.AddNotification("Auth", "Email ou senha inválidos.");
            return null;
        }

        var token = _tokenService.Generate(user.Id, user.Email, user.Role.ToString());

        return new AuthResponse(
            Token: token,
            Name: user.Name,
            Email: user.Email,
            Role: user.Role.ToString(),
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes));
    }
}