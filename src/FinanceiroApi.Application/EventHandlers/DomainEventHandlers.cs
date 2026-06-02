using AutoMapper;
using FinanceiroApi.Application.Interfaces;
using FinanceiroApi.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FinanceiroApi.Application.EventHandlers;

public class EmployeeCreatedEventHandler : INotificationHandler<EmployeeCreatedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmployeeCreatedEventHandler> _logger;

    public EmployeeCreatedEventHandler(IEmailSender emailSender, ILogger<EmployeeCreatedEventHandler> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Handle(EmployeeCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Enviando e-mail de boas-vindas para {Email}", notification.Email);

        var message = new EmailMessage(
            To: notification.Email,
            Subject: "Bem-vindo(a) à empresa!",
            HtmlBody: $"Olá, {notification.FullName}! Seu cadastro foi realizado com sucesso.");

        await _emailSender.SendAsync(message, cancellationToken);
    }
}

public class PayrollProcessedEventHandler : INotificationHandler<PayrollProcessedEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<PayrollProcessedEventHandler> _logger;

    public PayrollProcessedEventHandler(IEmailSender emailSender, ILogger<PayrollProcessedEventHandler> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task Handle(PayrollProcessedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Folha processada para o período {Period} — total líquido: {TotalNet:C}",
            notification.Period, notification.TotalNet);

        var message = new EmailMessage(
            To: "gestao@empresa.com",
            Subject: $"Folha para o período {notification.Period} processada",
            HtmlBody: $"A folha foi processada com sucesso. Total líquido: {notification.TotalNet:C}.");

        await _emailSender.SendAsync(message, cancellationToken);
    }
}

public class EmployeeSalaryUpdatedEventHandler : INotificationHandler<EmployeeSalaryUpdatedEvent>
{
    private readonly ILogger<EmployeeSalaryUpdatedEventHandler> _logger;

    public EmployeeSalaryUpdatedEventHandler(ILogger<EmployeeSalaryUpdatedEventHandler> logger) => _logger = logger;

    public Task Handle(EmployeeSalaryUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Salário do funcionário {EmployeeId} atualizado de {OldSalary:C} para {NewSalary:C}.",
            notification.EmployeeId, notification.OldSalary, notification.NewSalary);

        return Task.CompletedTask;
    }
}
