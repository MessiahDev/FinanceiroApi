using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class BankReconciliationItem : BaseEntity
{
    public Guid BankReconciliationId { get; private set; }
    public Guid BankStatementEntryId { get; private set; }
    public Guid? TransactionId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public ReconciliationItemStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public BankReconciliation? BankReconciliation { get; private set; }
    public BankStatementEntry? BankStatementEntry { get; private set; }
    public Transaction? Transaction { get; private set; }

    protected BankReconciliationItem() { }

    internal static BankReconciliationItem Create(
        Guid bankReconciliationId,
        Guid bankStatementEntryId,
        Guid? transactionId,
        decimal amount,
        ReconciliationItemStatus status,
        string? notes = null)
    {
        if (amount <= 0)
            throw new DomainException("Item amount must be greater than zero.");

        return new BankReconciliationItem
        {
            BankReconciliationId = bankReconciliationId,
            BankStatementEntryId = bankStatementEntryId,
            TransactionId = transactionId,
            Amount = new Money(amount),
            Status = status,
            Notes = notes?.Trim()
        };
    }

    public void Match(Guid transactionId)
    {
        if (Status == ReconciliationItemStatus.Matched)
            throw new DomainException("Item is already matched.");

        TransactionId = transactionId;
        Status = ReconciliationItemStatus.Matched;
        SetUpdatedAt();
    }

    public void MarkAsUnmatched(string reason)
    {
        Status = ReconciliationItemStatus.Unmatched;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | {reason}";
        SetUpdatedAt();
    }
}
