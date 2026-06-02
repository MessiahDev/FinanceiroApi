using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Security;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Options;

namespace FinanceiroApi.Application.Commands.Auth;

public sealed record RegisterCommand(
    string Name,
    string Email,
    string Password,
    UserRole Role) : IRequest<AuthResponse?>;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse?>
{
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;
    private readonly INotificationContext _notifications;
    private readonly JwtSettings _jwtSettings;

    public RegisterCommandHandler(
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

    public async Task<AuthResponse?> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            _notifications.AddNotification("Auth", "E-mail já cadastrado.");
            return null;
        }

        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.Name, request.Email, hash, request.Role);

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var token = _tokenService.Generate(user.Id, user.Email, user.Role.ToString());

        return new AuthResponse(
            Token: token,
            Name: user.Name,
            Email: user.Email,
            Role: user.Role.ToString(),
            ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes));
    }
}