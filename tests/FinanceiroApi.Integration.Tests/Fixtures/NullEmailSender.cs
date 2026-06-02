using FinanceiroApi.Application.Interfaces;

namespace FinanceiroApi.Integration.Tests.Fixtures;

public sealed class NullEmailSender : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task SendBulkAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
