namespace FinanceiroApi.Infrastructure.ExternalServices.Payment;

public sealed class PaymentSettings
{
    public const string SectionName = "Payment";

    public string BaseUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public bool UseSandbox { get; init; } = true;
}
