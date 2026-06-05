using FinanceiroApi.Domain.Entities;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Interfaces.Repositories;
using FinanceiroApi.Domain.Interfaces;
using MediatR;

namespace FinanceiroApi.Application.Commands.JournalEntries.CreateJournalEntry;

public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, Guid>
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IAccountingPeriodRepository _periodRepository;
    private readonly IChartOfAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateJournalEntryCommandHandler(
        IJournalEntryRepository journalEntryRepository,
        IAccountingPeriodRepository periodRepository,
        IChartOfAccountRepository accountRepository,
        IUnitOfWork unitOfWork)
    {
        _journalEntryRepository = journalEntryRepository;
        _periodRepository = periodRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        var period = await _periodRepository.GetByIdAsync(request.AccountingPeriodId, cancellationToken)
            ?? throw new DomainException($"PerÃ­odo contÃ¡bil '{request.AccountingPeriodId}' nÃ£o encontrado.");

        if (!period.AcceptsEntries())
            throw new AccountingPeriodClosedException(period.Name);

        var entryDateOnly = DateOnly.FromDateTime(request.EntryDate);
        if (entryDateOnly < period.Period.Start || entryDateOnly > period.Period.End)
            throw new DomainException(
                $"A data do lanÃ§amento ({request.EntryDate:dd/MM/yyyy}) estÃ¡ fora do perÃ­odo " +
                $"'{period.Name}' ({period.Period.Start:dd/MM/yyyy} a {period.Period.End:dd/MM/yyyy}).");

        var entryNumber = await _journalEntryRepository.GetNextEntryNumberAsync(request.EntryDate.Year, cancellationToken);

        var entry = JournalEntry.Create(
            entryNumber,
            request.Description,
            request.EntryDate,
            request.EntryType,
            request.AccountingPeriodId,
            request.CreatedByUserId,
            request.ReferenceDocument,
            request.ReferenceDocumentType,
            request.ReferenceDocumentId);

        foreach (var line in request.Lines)
        {
            var account = await _accountRepository.GetByIdAsync(line.ChartOfAccountId, cancellationToken)
                ?? throw new DomainException($"Conta contÃ¡bil '{line.ChartOfAccountId}' nÃ£o encontrada.");

            if (!account.AcceptsEntries)
                throw new AccountNotAcceptingEntriesException(account.Code, account.Name);

            if (!account.IsActive)
                throw new DomainException($"A conta '{account.Code} - {account.Name}' estÃ¡ inativa.");

            entry.AddLine(line.ChartOfAccountId, line.DebitCredit, line.Amount, line.Description);
        }

        await _journalEntryRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return entry.Id;
    }
}
