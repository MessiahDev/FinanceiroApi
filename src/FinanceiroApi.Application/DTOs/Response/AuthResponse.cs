namespace FinanceiroApi.Application.DTOs.Response;

public sealed record AuthResponse(
    string Token,
    string Name,
    string Email,
    string Role,
    DateTime ExpiresAt);