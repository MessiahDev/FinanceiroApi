using FinanceiroApi.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace FinanceiroApi.Infrastructure.ExternalServices.Email;

public sealed class NullEmailSender : IEmailSender
{
    private readonly ILogger<NullEmailSender> _logger;

    public NullEmailSender(ILogger<NullEmailSender> logger) => _logger = logger;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Email não enviado (SendGrid não configurado): {To} | {Subject}", message.To, message.Subject);
        return Task.CompletedTask;
    }

    public Task SendBulkAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default)
    {
        foreach (var message in messages)
            _logger.LogWarning("Email não enviado (SendGrid não configurado): {To} | {Subject}", message.To, message.Subject);
        return Task.CompletedTask;
    }
}