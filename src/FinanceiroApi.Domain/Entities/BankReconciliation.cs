using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class BankReconciliation : AggregateRoot
{
    public Guid BankAccountId { get; private set; }
    public Guid BankStatementId { get; private set; }
    public DateOnly PeriodStart { get; private set; }
    public DateOnly PeriodEnd { get; private set; }
    public Money StatementOpeningBalance { get; private set; } = default!;
    public Money StatementClosingBalance { get; private set; } = default!;
    public Money SystemBalance { get; private set; } = default!;
    public Money Difference => StatementClosingBalance - SystemBalance;
    public ReconciliationStatus Status { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? CompletedBy { get; private set; }
    public string? Notes { get; private set; }

    public BankAccount? BankAccount { get; private set; }
    public BankStatement? BankStatement { get; private set; }
    public IReadOnlyList<BankReconciliationItem> Items => _items.AsReadOnly();
    private readonly List<BankReconciliationItem> _items = [];

    protected BankReconciliation() { }

    public static BankReconciliation Create(
        Guid bankAccountId,
        Guid bankStatementId,
        DateOnly periodStart,
        DateOnly periodEnd,
        decimal statementOpeningBalance,
        decimal statementClosingBalance,
        decimal systemBalance,
        string? notes = null)
    {
        if (periodEnd < periodStart)
            throw new DomainException("Period end cannot be before period start.");

        var reconciliation = new BankReconciliation
        {
            BankAccountId = bankAccountId,
            BankStatementId = bankStatementId,
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            StatementOpeningBalance = new Money(statementOpeningBalance),
            StatementClosingBalance = new Money(statementClosingBalance),
            SystemBalance = new Money(systemBalance),
            Status = ReconciliationStatus.Open,
            Notes = notes?.Trim()
        };

        reconciliation.AddDomainEvent(new BankReconciliationCreatedEvent(
            reconciliation.Id, bankAccountId, periodStart, periodEnd));
        return reconciliation;
    }

    public BankReconciliationItem AddItem(
        Guid bankStatementEntryId,
        Guid? transactionId,
        decimal amount,
        ReconciliationItemStatus itemStatus,
        string? notes = null)
    {
        if (Status == ReconciliationStatus.Completed)
            throw new DomainException("Cannot add items to a completed reconciliation.");
        if (Status == ReconciliationStatus.Cancelled)
            throw new DomainException("Cannot add items to a cancelled reconciliation.");

        var item = BankReconciliationItem.Create(Id, bankStatementEntryId, transactionId, amount, itemStatus, notes);
        _items.Add(item);

        if (Status == ReconciliationStatus.Open)
        {
            Status = ReconciliationStatus.InProgress;
        }

        SetUpdatedAt();
        return item;
    }

    public void Complete(Guid completedBy)
    {
        if (Status == ReconciliationStatus.Completed)
            throw new DomainException("Reconciliation is already completed.");
        if (Status == ReconciliationStatus.Cancelled)
            throw new DomainException("Cancelled reconciliations cannot be completed.");

        var hasUnmatched = _items.Any(i => i.Status == ReconciliationItemStatus.Pending);
        if (hasUnmatched)
            throw new DomainException("Cannot complete reconciliation with pending items.");

        Status = ReconciliationStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = completedBy;
        SetUpdatedAt();

        AddDomainEvent(new BankReconciliationCompletedEvent(Id, BankAccountId, Difference.Amount));
    }

    public void Cancel(string reason)
    {
        if (Status == ReconciliationStatus.Completed)
            throw new DomainException("Completed reconciliations cannot be cancelled.");

        Status = ReconciliationStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | Cancelled: {reason}";
        SetUpdatedAt();
    }

    public bool IsBalanced => Difference.Amount == 0;
    public int TotalItems => _items.Count;
    public int MatchedItems => _items.Count(i => i.Status == ReconciliationItemStatus.Matched);
    public int UnmatchedItems => _items.Count(i => i.Status == ReconciliationItemStatus.Unmatched);
}
