using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class AccountPayable : AggregateRoot
{
    public Guid SupplierId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public string Description { get; private set; } = default!;
    public Money TotalAmount { get; private set; } = default!;
    public Money PaidAmount { get; private set; } = Money.Zero;
    public Money RemainingAmount => TotalAmount - PaidAmount;
    public DateOnly DueDate { get; private set; }
    public DateOnly? PaymentDate { get; private set; }
    public AccountPayableStatus Status { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public string? Notes { get; private set; }
    public Guid? BankAccountId { get; private set; }

    public Supplier? Supplier { get; private set; }
    public CostCenter? CostCenter { get; private set; }

    protected AccountPayable() { }

    public static AccountPayable Create(
        Guid supplierId,
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

        var payable = new AccountPayable
        {
            SupplierId = supplierId,
            CostCenterId = costCenterId,
            Description = description.Trim(),
            TotalAmount = new Money(totalAmount),
            DueDate = dueDate,
            Status = AccountPayableStatus.Pending,
            InvoiceNumber = invoiceNumber?.Trim(),
            Notes = notes?.Trim()
        };

        payable.AddDomainEvent(new AccountPayableCreatedEvent(payable.Id, supplierId, payable.TotalAmount, dueDate));
        return payable;
    }

    public void RegisterPayment(decimal amount, DateOnly paymentDate, Guid bankAccountId)
    {
        if (Status == AccountPayableStatus.Paid)
            throw new DomainException("Account payable is already fully paid.");
        if (Status == AccountPayableStatus.Cancelled)
            throw new DomainException("Cancelled accounts cannot be paid.");
        if (amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.");
        if (new Money(amount) > RemainingAmount)
            throw new DomainException("Payment amount exceeds remaining balance.");

        PaidAmount = PaidAmount + new Money(amount);
        BankAccountId = bankAccountId;
        PaymentDate = paymentDate;

        Status = PaidAmount == TotalAmount
            ? AccountPayableStatus.Paid
            : AccountPayableStatus.PartiallyPaid;

        SetUpdatedAt();
        AddDomainEvent(new AccountPayablePaidEvent(Id, SupplierId, new Money(amount), Status));
    }

    public void MarkAsOverdue()
    {
        if (Status != AccountPayableStatus.Pending && Status != AccountPayableStatus.PartiallyPaid)
            return;
        if (DueDate >= DateOnly.FromDateTime(DateTime.UtcNow))
            return;

        Status = AccountPayableStatus.Overdue;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status == AccountPayableStatus.Paid)
            throw new DomainException("Paid accounts cannot be cancelled.");

        Status = AccountPayableStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | Cancelled: {reason}";
        SetUpdatedAt();
    }
}