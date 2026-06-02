using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceiroApi.Infrastructure.ExternalServices.Payment;

public sealed class PaymentGatewayService : IPaymentGateway
{
    private readonly PaymentSettings _settings;
    private readonly ILogger<PaymentGatewayService> _logger;
    private readonly HttpClient _http;

    public PaymentGatewayService(
        HttpClient http,
        IOptions<PaymentSettings> settings,
        ILogger<PaymentGatewayService> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessAsync(
        PaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing payment for {Reference} | Amount: {Amount} {Currency}",
            request.ExternalReference, request.Amount, request.Currency);

        await Task.Delay(100, cancellationToken);

        return new PaymentResult(
            Success: true,
            TransactionId: Guid.NewGuid().ToString("N"),
            PixKey: null,
            PixQrCode: null);
    }

    public async Task<PaymentResult> RefundAsync(
        string transactionId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refunding transaction {TransactionId} | Amount: {Amount}", transactionId, amount);

        await Task.Delay(100, cancellationToken);

        return new PaymentResult(Success: true, TransactionId: transactionId);
    }

    public async Task<PaymentStatus> GetStatusAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        await Task.Delay(50, cancellationToken);

        return PaymentStatus.Completed;
    }
}
