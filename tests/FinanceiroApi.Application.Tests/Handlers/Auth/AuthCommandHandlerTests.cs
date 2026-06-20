using FinanceiroApi.Application.Commands.Auth.Login;
using FinanceiroApi.Application.Commands.Auth.Register;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Security;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace FinanceiroApi.Application.Tests.Handlers.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();
    private readonly TokenService _tokenService;
    private readonly IOptions<JwtSettings> _jwtOptions;

    public LoginCommandHandlerTests()
    {
        var settings = new JwtSettings
        {
            SecretKey = "super-secret-key-for-tests-1234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiresInMinutes = 60
        };
        _jwtOptions = Options.Create(settings);
        _tokenService = new TokenService(_jwtOptions);
    }

    private LoginCommandHandler CreateHandler() =>
        new(_repo, _tokenService, _notif, _jwtOptions);

    private static User MakeActiveUser(string email = "user@test.com", string password = "senha123")
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return User.Create("Usuário Teste", email, hash, UserRole.Admin);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnAuthResponse()
    {
        var user = MakeActiveUser();
        _repo.GetByEmailAsync(user.Email, default).Returns(user);

        var result = await CreateHandler().Handle(new LoginCommand(user.Email, "senha123"), default);

        result.Should().NotBeNull();
        result!.Email.Should().Be(user.Email);
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldNotifyAndReturnNull()
    {
        _repo.GetByEmailAsync(Arg.Any<string>(), default).ReturnsNull();

        var result = await CreateHandler().Handle(new LoginCommand("x@x.com", "senha"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Auth", Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldNotifyAndReturnNull()
    {
        var user = MakeActiveUser();
        _repo.GetByEmailAsync(user.Email, default).Returns(user);

        var result = await CreateHandler().Handle(new LoginCommand(user.Email, "senha-errada"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Auth", Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldNotifyAndReturnNull()
    {
        var user = MakeActiveUser();
        user.Deactivate(Guid.NewGuid());
        _repo.GetByEmailAsync(user.Email, default).Returns(user);

        var result = await CreateHandler().Handle(new LoginCommand(user.Email, "senha123"), default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Auth", Arg.Any<string>());
    }
}

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly INotificationContext _notif = Substitute.For<INotificationContext>();
    private readonly TokenService _tokenService;
    private readonly IOptions<JwtSettings> _jwtOptions;

    public RegisterCommandHandlerTests()
    {
        var settings = new JwtSettings
        {
            SecretKey = "super-secret-key-for-tests-1234567890",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiresInMinutes = 60
        };
        _jwtOptions = Options.Create(settings);
        _tokenService = new TokenService(_jwtOptions);
    }

    private RegisterCommandHandler CreateHandler() =>
        new(_repo, _tokenService, _notif, _jwtOptions);

    [Fact]
    public async Task Handle_NewEmail_ShouldCreateUserAndReturnAuthResponse()
    {
        _repo.ExistsByEmailAsync("novo@test.com", default).Returns(false);

        var cmd = new RegisterCommand("Novo Usuário", "novo@test.com", "senha123", UserRole.Employee);
        var result = await CreateHandler().Handle(cmd, default);

        result.Should().NotBeNull();
        result!.Email.Should().Be("novo@test.com");
        result.Token.Should().NotBeNullOrEmpty();
        await _repo.Received(1).AddAsync(Arg.Any<User>(), default);
        await _repo.Received(1).SaveChangesAsync(default);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldNotifyAndReturnNull()
    {
        _repo.ExistsByEmailAsync(Arg.Any<string>(), default).Returns(true);

        var cmd = new RegisterCommand("Usuário", "existente@test.com", "senha123", UserRole.Employee);
        var result = await CreateHandler().Handle(cmd, default);

        result.Should().BeNull();
        _notif.Received(1).AddNotification("Auth", Arg.Any<string>());
        await _repo.DidNotReceive().AddAsync(Arg.Any<User>(), default);
    }
}
