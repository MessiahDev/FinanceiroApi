namespace FinanceiroApi.Infrastructure.ExternalServices.Payment;

public interface IPaymentGateway
{
    Task<PaymentResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentResult> RefundAsync(string transactionId, decimal amount, CancellationToken cancellationToken = default);
    Task<PaymentStatus> GetStatusAsync(string transactionId, CancellationToken cancellationToken = default);
}

public sealed record PaymentRequest(
    string ExternalReference,
    decimal Amount,
    string Currency,
    string Description,
    string RecipientDocument,
    string RecipientName,
    string BankCode,
    string AgencyNumber,
    string AccountNumber,
    string AccountDigit,
    PaymentMethod Method = PaymentMethod.Pix);

public sealed record PaymentResult(
    bool Success,
    string TransactionId,
    string? ErrorMessage = null,
    string? PixKey = null,
    string? PixQrCode = null);

public enum PaymentMethod { Pix, Ted, Doc }
public enum PaymentStatus { Pending, Processing, Completed, Failed, Refunded, Cancelled }
