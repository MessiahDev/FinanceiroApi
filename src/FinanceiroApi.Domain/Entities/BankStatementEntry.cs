using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class BankStatementEntry : BaseEntity
{
    public Guid BankStatementId { get; private set; }
    public DateOnly Date { get; private set; }
    public string Description { get; private set; } = default!;
    public Money Amount { get; private set; } = default!;
    public BankStatementEntryType EntryType { get; private set; }
    public string? DocumentNumber { get; private set; }
    public bool IsReconciled { get; private set; }
    public Guid? ReconciliationItemId { get; private set; }

    public BankStatement? BankStatement { get; private set; }

    protected BankStatementEntry() { }

    internal static BankStatementEntry Create(
        Guid bankStatementId,
        DateOnly date,
        string description,
        decimal amount,
        BankStatementEntryType entryType,
        string? documentNumber = null)
    {
        return new BankStatementEntry
        {
            BankStatementId = bankStatementId,
            Date = date,
            Description = description.Trim(),
            Amount = new Money(amount),
            EntryType = entryType,
            DocumentNumber = documentNumber?.Trim(),
            IsReconciled = false
        };
    }

    internal void MarkAsReconciled(Guid reconciliationItemId)
    {
        IsReconciled = true;
        ReconciliationItemId = reconciliationItemId;
        SetUpdatedAt();
    }

    internal void UnmarkReconciliation()
    {
        IsReconciled = false;
        ReconciliationItemId = null;
        SetUpdatedAt();
    }
}
