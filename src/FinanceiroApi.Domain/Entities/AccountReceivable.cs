using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class AccountReceivable : AggregateRoot
{
    public Guid CustomerId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public string Description { get; private set; } = default!;
    public Money TotalAmount { get; private set; } = default!;
    public Money ReceivedAmount { get; private set; } = Money.Zero;
    public Money RemainingAmount => TotalAmount - ReceivedAmount;
    public DateOnly DueDate { get; private set; }
    public DateOnly? ReceiptDate { get; private set; }
    public AccountReceivableStatus Status { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid? BankAccountId { get; private set; }

    public Customer? Customer { get; private set; }
    public CostCenter? CostCenter { get; private set; }

    protected AccountReceivable() { }

    public static AccountReceivable Create(
        Guid customerId,
        string description,
        decimal totalAmount,
        DateOnly dueDate,
        Guid? costCenterId = null,
        string? invoiceNumber = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");
        if (totalAmount <= 0)
            throw new DomainException("Total amount must be greater than zero.");

        var receivable = new AccountReceivable
        {
            CustomerId = customerId,
            CostCenterId = costCenterId,
            Description = description.Trim(),
            TotalAmount = new Money(totalAmount),
            DueDate = dueDate,
            Status = AccountReceivableStatus.Pending,
            InvoiceNumber = invoiceNumber?.Trim(),
            Notes = notes?.Trim()
        };

        receivable.AddDomainEvent(new AccountReceivableCreatedEvent(receivable.Id, customerId, receivable.TotalAmount, dueDate));
        return receivable;
    }

    public void RegisterReceipt(decimal amount, DateOnly receiptDate, Guid bankAccountId)
    {
        if (Status == AccountReceivableStatus.Received)
            throw new DomainException("Account receivable is already fully received.");
        if (Status == AccountReceivableStatus.Cancelled)
            throw new DomainException("Cancelled accounts cannot be received.");
        if (amount <= 0)
            throw new DomainException("Receipt amount must be greater than zero.");
        if (new Money(amount) > RemainingAmount)
            throw new DomainException("Receipt amount exceeds remaining balance.");

        ReceivedAmount = ReceivedAmount + new Money(amount);
        BankAccountId = bankAccountId;
        ReceiptDate = receiptDate;

        Status = ReceivedAmount == TotalAmount
            ? AccountReceivableStatus.Received
            : AccountReceivableStatus.PartiallyReceived;

        SetUpdatedAt();
        AddDomainEvent(new AccountReceivableReceivedEvent(Id, CustomerId, new Money(amount), Status));
    }

    public void MarkAsOverdue()
    {
        if (Status != AccountReceivableStatus.Pending && Status != AccountReceivableStatus.PartiallyReceived)
            return;
        if (DueDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            return;

        Status = AccountReceivableStatus.Overdue;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status == AccountReceivableStatus.Received)
            throw new DomainException("Received accounts cannot be cancelled.");

        Status = AccountReceivableStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | Cancelled: {reason}";
        SetUpdatedAt();
    }
}