using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class BankStatement : AggregateRoot
{
    public Guid BankAccountId { get; private set; }
    public DateOnly StatementDate { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public Money OpeningBalance { get; private set; } = default!;
    public Money ClosingBalance { get; private set; } = default!;
    public BankStatementStatus Status { get; private set; }
    public string? FileName { get; private set; }
    public string? Notes { get; private set; }

    public BankAccount? BankAccount { get; private set; }
    public IReadOnlyList<BankStatementEntry> Entries => _entries.AsReadOnly();
    private readonly List<BankStatementEntry> _entries = [];

    protected BankStatement() { }

    public static BankStatement Create(
        Guid bankAccountId,
        DateOnly statementDate,
        DateOnly periodStart,
        DateOnly periodEnd,
        decimal openingBalance,
        decimal closingBalance,
        string? fileName = null,
        string? notes = null)
    {
        if (periodEnd < periodStart)
            throw new DomainException("Period end cannot be before period start.");

        var statement = new BankStatement
        {
            BankAccountId = bankAccountId,
            StatementDate = statementDate,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            OpeningBalance = new Money(openingBalance),
            ClosingBalance = new Money(closingBalance),
            Status = BankStatementStatus.Imported,
            FileName = fileName?.Trim(),
            Notes = notes?.Trim()
        };

        statement.AddDomainEvent(new BankStatementImportedEvent(statement.Id, bankAccountId, periodStart, periodEnd));
        return statement;
    }

    public BankStatementEntry AddEntry(
        DateOnly date,
        string description,
        decimal amount,
        BankStatementEntryType entryType,
        string? documentNumber = null)
    {
        if (Status == BankStatementStatus.Cancelled)
            throw new DomainException("Cannot add entries to a cancelled statement.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Entry description is required.");
        if (amount <= 0)
            throw new DomainException("Entry amount must be greater than zero.");

        var entry = BankStatementEntry.Create(Id, date, description, amount, entryType, documentNumber);
        _entries.Add(entry);
        SetUpdatedAt();
        return entry;
    }

    public void MarkAsReconciled()
    {
        if (Status == BankStatementStatus.Cancelled)
            throw new DomainException("Cancelled statements cannot be reconciled.");

        Status = BankStatementStatus.Reconciled;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status == BankStatementStatus.Reconciled)
            throw new DomainException("Reconciled statements cannot be cancelled.");

        Status = BankStatementStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | Cancelled: {reason}";
        SetUpdatedAt();
    }

    public int TotalEntries => _entries.Count;
    public Money TotalCredits => new(_entries.Where(e => e.EntryType == BankStatementEntryType.Credit).Sum(e => e.Amount.Amount));
    public Money TotalDebits => new(_entries.Where(e => e.EntryType == BankStatementEntryType.Debit).Sum(e => e.Amount.Amount));
}
