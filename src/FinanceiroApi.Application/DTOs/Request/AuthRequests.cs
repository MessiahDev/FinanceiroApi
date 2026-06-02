using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.Application.DTOs.Request;

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RegisterRequest(
    string Name,
    string Email,
    string Password,
    UserRole Role);