namespace FinanceiroApi.API.Settings;

public class JwtSettings
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiresInMinutes { get; init; } = 480;
}

public class AppSettings
{
    public string ApplicationName { get; init; } = "FinanceiroApi";
    public string Environment { get; init; } = "Production";
}
