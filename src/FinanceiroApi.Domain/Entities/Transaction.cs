using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class Transaction : AggregateRoot
{
    public Money Amount { get; private set; } = default!;
    public TransactionType Type { get; private set; }
    public TransactionCategory Category { get; private set; }
    public TransactionStatus Status { get; private set; }
    public string Description { get; private set; } = default!;
    public DateOnly TransactionDate { get; private set; }
    public Guid? EmployeeId { get; private set; }
    public Guid? PayrollId { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public string? Notes { get; private set; }

    public Employee? Employee { get; private set; }

    protected Transaction() { }

    public static Transaction Create(
        decimal amount,
        TransactionType type,
        TransactionCategory category,
        string description,
        DateOnly? transactionDate = null,
        Guid? employeeId = null,
        Guid? payrollId = null,
        string? referenceNumber = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Transaction description is required.");

        var transaction = new Transaction
        {
            Amount = new Money(amount),
            Type = type,
            Category = category,
            Status = TransactionStatus.Pending,
            Description = description.Trim(),
            TransactionDate = transactionDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EmployeeId = employeeId,
            PayrollId = payrollId,
            ReferenceNumber = referenceNumber,
            Notes = notes
        };

        transaction.AddDomainEvent(new TransactionCreatedEvent(transaction.Id, transaction.Amount, type, category));
        return transaction;
    }

    public void Confirm()
    {
        if (Status != TransactionStatus.Pending)
            throw new DomainException("Only pending transactions can be confirmed.");

        Status = TransactionStatus.Confirmed;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status == TransactionStatus.Confirmed)
            throw new DomainException("Confirmed transactions cannot be cancelled.");

        Status = TransactionStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | Cancelled: {reason}";
        SetUpdatedAt();
    }
}
