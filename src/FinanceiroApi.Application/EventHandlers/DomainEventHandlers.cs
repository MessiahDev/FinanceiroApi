using MediatR;
using Microsoft.Extensions.Logging;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Application.Interfaces;

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
            Subject: "Bem-vindo(a) Ã  empresa!",
            HtmlBody: $"OlÃ¡, {notification.FullName}! Seu cadastro foi realizado com sucesso.");

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
            "Folha processada para o perÃ­odo {Period} â€” total lÃ­quido: {TotalNet:C}",
            notification.Period, notification.TotalNet);

        var message = new EmailMessage(
            To: "gestao@empresa.com",
            Subject: $"Folha para o perÃ­odo {notification.Period} processada",
            HtmlBody: $"A folha foi processada com sucesso. Total lÃ­quido: {notification.TotalNet:C}.");

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
            "SalÃ¡rio do funcionÃ¡rio {EmployeeId} atualizado de {OldSalary:C} para {NewSalary:C}.",
            notification.EmployeeId, notification.OldSalary, notification.NewSalary);

        return Task.CompletedTask;
    }
}

public class AccountPayablePaidDomainEventHandler : INotificationHandler<AccountPayablePaidEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly IChartOfAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountPayablePaidDomainEventHandler> _logger;

    private const string AccountsPayableAccountCode = "2.1.01.001";
    private const string BankAccountCode = "1.1.01.001";

    public AccountPayablePaidDomainEventHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountingPeriodRepository periodRepository,
        IChartOfAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<AccountPayablePaidDomainEventHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _periodRepository = periodRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AccountPayablePaidEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var period = await _periodRepository.GetCurrentOpenPeriodAsync(cancellationToken);
            if (period is null)
            {
                _logger.LogWarning("Nenhum perÃ­odo contÃ¡bil aberto para lanÃ§amento automÃ¡tico de conta a pagar {Id}",
                    notification.Id);
                return;
            }

            var payableAccount = await _accountRepository.GetByCodeAsync(AccountsPayableAccountCode, cancellationToken);
            var bankAccount = await _accountRepository.GetByCodeAsync(BankAccountCode, cancellationToken);

            if (payableAccount is null || bankAccount is null)
            {
                _logger.LogWarning("Contas contÃ¡beis padrÃ£o nÃ£o configuradas para lanÃ§amento automÃ¡tico de pagamento.");
                return;
            }

            var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(DateTime.UtcNow.Year, cancellationToken);

            var entry = JournalEntry.Create(
                entryNumber,
                $"Pagamento de conta a pagar - {"Pagamento"}",
                DateTime.UtcNow,
                JournalEntryType.AccountsPayable,
                period.Id,
                Guid.Empty,
                null,
                nameof(AccountPayable),
                notification.Id);

            entry.AddLine(payableAccount.Id, DebitCredit.Debit, notification.PaidAmount.Amount);

            entry.AddLine(bankAccount.Id, DebitCredit.Credit, notification.PaidAmount.Amount);

            entry.Post();

            await _journalEntryRepository.AddAsync(entry, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("LanÃ§amento contÃ¡bil {Number} gerado para pagamento de conta a pagar {Id}",
                entryNumber, notification.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar lanÃ§amento contÃ¡bil para conta a pagar {Id}",
                notification.Id);
        }
    }
}

public class AccountReceivableReceivedDomainEventHandler : INotificationHandler<AccountReceivableReceivedEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly IChartOfAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountReceivableReceivedDomainEventHandler> _logger;

    private const string AccountsReceivableAccountCode = "1.1.02.001";
    private const string BankAccountCode = "1.1.01.001";

    public AccountReceivableReceivedDomainEventHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountingPeriodRepository periodRepository,
        IChartOfAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<AccountReceivableReceivedDomainEventHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _periodRepository = periodRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(AccountReceivableReceivedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var period = await _periodRepository.GetCurrentOpenPeriodAsync(cancellationToken);
            if (period is null) return;

            var receivableAccount = await _accountRepository.GetByCodeAsync(AccountsReceivableAccountCode, cancellationToken);
            var bankAccount = await _accountRepository.GetByCodeAsync(BankAccountCode, cancellationToken);

            if (receivableAccount is null || bankAccount is null) return;

            var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(DateTime.UtcNow.Year, cancellationToken);

            var entry = JournalEntry.Create(
                entryNumber,
                $"Recebimento de conta a receber - {"Pagamento"}",
                DateTime.UtcNow,
                JournalEntryType.AccountsReceivable,
                period.Id,
                Guid.Empty,
                null,
                nameof(AccountReceivable),
                notification.Id);

            entry.AddLine(bankAccount.Id, DebitCredit.Debit, notification.ReceivedAmount.Amount);

            entry.AddLine(receivableAccount.Id, DebitCredit.Credit, notification.ReceivedAmount.Amount);

            entry.Post();

            await _journalEntryRepository.AddAsync(entry, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar lanÃ§amento contÃ¡bil para recebimento {Id}",
                notification.Id);
        }
    }
}

public class PayrollProcessedDomainEventHandler : INotificationHandler<PayrollProcessedEvent>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly IChartOfAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PayrollProcessedDomainEventHandler> _logger;

    private const string SalaryExpenseCode = "3.1.01.001";
    private const string SocialChargesCode = "3.1.01.002";
    private const string PayrollPayableCode = "2.1.02.001";

    public PayrollProcessedDomainEventHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountingPeriodRepository periodRepository,
        IChartOfAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        ILogger<PayrollProcessedDomainEventHandler> logger)
    {
        _journalEntryRepository = journalEntryRepository;
        _periodRepository = periodRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(PayrollProcessedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var period = await _periodRepository.GetCurrentOpenPeriodAsync(cancellationToken);
            if (period is null) return;

            var salaryAccount = await _accountRepository.GetByCodeAsync(SalaryExpenseCode, cancellationToken);
            var chargesAccount = await _accountRepository.GetByCodeAsync(SocialChargesCode, cancellationToken);
            var payableAccount = await _accountRepository.GetByCodeAsync(PayrollPayableCode, cancellationToken);

            if (salaryAccount is null || payableAccount is null) return;

            var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(DateTime.UtcNow.Year, cancellationToken);
            var totalPayroll = notification.TotalNet.Amount;

            var entry = JournalEntry.Create(
                entryNumber,
                $"Folha de Pagamento - {DateTime.UtcNow:MM/yyyy}",
                DateTime.UtcNow,
                JournalEntryType.Payroll,
                period.Id,
                Guid.Empty,
                null,
                nameof(Payroll),
                notification.PayrollId);

            entry.AddLine(salaryAccount.Id, DebitCredit.Debit, notification.TotalNet.Amount);


            entry.AddLine(payableAccount.Id, DebitCredit.Credit, totalPayroll);

            entry.Post();

            await _journalEntryRepository.AddAsync(entry, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar lanÃ§amento contÃ¡bil para folha {Id}", notification.PayrollId);
        }
    }
}



