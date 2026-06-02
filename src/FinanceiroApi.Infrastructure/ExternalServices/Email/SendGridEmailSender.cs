using FinanceiroApi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FinanceiroApi.Infrastructure.ExternalServices.Email;

public sealed class SendGridEmailSender : IEmailSender
{
    private readonly SendGridClient _client;
    private readonly EmailSettings _settings;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(IOptions<EmailSettings> settings, ILogger<SendGridEmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _client = new SendGridClient(_settings.ApiKey);
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            var msg = BuildMessage(message);
            var response = await _client.SendEmailAsync(msg, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("SendGrid returned {StatusCode}: {Body}", response.StatusCode, body);
                throw new InvalidOperationException($"Failed to send email. Status: {response.StatusCode}");
            }

            _logger.LogInformation("Email sent to {To} | Subject: {Subject}", message.To, message.Subject);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error sending email to {To}", message.To);
            throw;
        }
    }

    public async Task SendBulkAsync(IEnumerable<EmailMessage> messages, CancellationToken cancellationToken = default)
    {
        var tasks = messages.Select(m => SendAsync(m, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private SendGridMessage BuildMessage(EmailMessage message)
    {
        var msg = new SendGridMessage
        {
            From = new EmailAddress(_settings.FromEmail, _settings.FromName),
            Subject = message.Subject,
            HtmlContent = message.HtmlBody,
            PlainTextContent = message.PlainTextBody
        };

        msg.AddTo(new EmailAddress(message.To, message.ToName));

        if (message.Attachments is { Count: > 0 })
        {
            foreach (var attachment in message.Attachments)
            {
                msg.AddAttachment(
                    attachment.FileName,
                    Convert.ToBase64String(attachment.Content),
                    attachment.ContentType);
            }
        }

        return msg;
    }
}
