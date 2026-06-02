using FinanceiroApi.Application.DTOs.Response;

namespace FinanceiroApi.Application.Interfaces;

public sealed record EmailAttachment(
    string FileName,
    byte[] Content,
    string ContentType = "application/octet-stream");

public sealed record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? ToName = null,
    string? PlainTextBody = null,
    IReadOnlyList<EmailAttachment>? Attachments = null);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
    Task SendBulkAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
}

public interface IEventBusPublisher
{
    Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class;
}

public interface IFinancialReportService
{
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync(
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);
}

public interface IPayslipGeneratorService
{
    Task<byte[]> GeneratePdfAsync(
        PayrollResponse payroll,
        EmployeeResponse employee,
        CancellationToken cancellationToken = default);
}
