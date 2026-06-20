using System.Security.Claims;
using Microsoft.AspNetCore.Http;
namespace FinanceiroApi.CrossCutting.Services;

public sealed class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;
    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    public Guid UserId =>
        Guid.TryParse(User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;
    public string Email =>
        User?.FindFirstValue("email") ?? User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;
}