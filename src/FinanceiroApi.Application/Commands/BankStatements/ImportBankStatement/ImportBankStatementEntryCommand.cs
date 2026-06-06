using AutoMapper;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Interfaces;
using FinanceiroApi.Domain.Interfaces.Repositories;
using MediatR;

namespace FinanceiroApi.Application.Commands.BankStatements.ImportBankStatement;

public record ImportBankStatementEntryCommand(
    DateOnly Date,
    string Description,
    decimal Amount,
    BankStatementEntryType EntryType,
    string? DocumentNumber);

public record ImportBankStatementCommand(
    Guid BankAccountId,
    DateOnly StatementDate,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal OpeningBalance,
    decimal ClosingBalance,
    string? FileName,
    string? Notes,
    List<ImportBankStatementEntryCommand> Entries) : IRequest<BankStatementResponse>;

public class ImportBankStatementCommandHandler : IRequestHandler<ImportBankStatementCommand, BankStatementResponse>
{
    private readonly IBankStatementRepository _bankStatementRepository;
    private readonly IBankAccountRepository _bankAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationContext _notifications;

    public ImportBankStatementCommandHandler(
        IBankStatementRepository bankStatementRepository,
        IBankAccountRepository bankAccountRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        INotificationContext notifications)
    {
        _bankStatementRepository = bankStatementRepository;
        _bankAccountRepository = bankAccountRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifications = notifications;
    }

    public async Task<BankStatementResponse> Handle(ImportBankStatementCommand request, CancellationToken cancellationToken)
    {
        var bankAccountExists = await _bankAccountRepository.ExistsAsync(request.BankAccountId, cancellationToken);
        if (!bankAccountExists)
        {
            _notifications.AddNotification("BankAccountId", "Conta bancária não encontrada.");
            return null!;
        }

        var alreadyExists = await _bankStatementRepository.ExistsForPeriodAsync(
            request.BankAccountId, request.PeriodStart, request.PeriodEnd, cancellationToken);

        if (alreadyExists)
        {
            _notifications.AddNotification("Period", "Já existe um extrato importado para este período e conta bancária.");
            return null!;
        }

        var statement = BankStatement.Create(
            request.BankAccountId,
            request.StatementDate,
            request.PeriodStart,
            request.PeriodEnd,
            request.OpeningBalance,
            request.ClosingBalance,
            request.FileName,
            request.Notes);

        foreach (var entry in request.Entries)
        {
            statement.AddEntry(entry.Date, entry.Description, entry.Amount, entry.EntryType, entry.DocumentNumber);
        }

        await _bankStatementRepository.AddAsync(statement, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        var result = await _bankStatementRepository.GetWithEntriesAsync(statement.Id, cancellationToken);
        return _mapper.Map<BankStatementResponse>(result!);
    }
}
