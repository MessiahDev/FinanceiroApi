using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class TaxPayment : AggregateRoot
{
    public Guid TaxEntryId { get; private set; }
    public Guid BankAccountId { get; private set; }
    public Money Amount { get; private set; } = default!;
    public Money Fine { get; private set; } = Money.Zero;
    public Money Interest { get; private set; } = Money.Zero;
    public Money TotalPaid => Amount + Fine + Interest;
    public DateOnly PaymentDate { get; private set; }
    public string? DarfNumber { get; private set; }
    public string? ReceiptCode { get; private set; }
    public TaxPaymentStatus Status { get; private set; }
    public string? Notes { get; private set; }

    public TaxEntry? TaxEntry { get; private set; }
    public BankAccount? BankAccount { get; private set; }

    protected TaxPayment() { }

    public static TaxPayment Create(
        Guid taxEntryId,
        Guid bankAccountId,
        decimal amount,
        DateOnly paymentDate,
        decimal fine = 0,
        decimal interest = 0,
        string? darfNumber = null,
        string? receiptCode = null,
        string? notes = null)
    {
        if (amount <= 0)
            throw new DomainException("Payment amount must be greater than zero.");
        if (fine < 0)
            throw new DomainException("Fine cannot be negative.");
        if (interest < 0)
            throw new DomainException("Interest cannot be negative.");

        var payment = new TaxPayment
        {
            TaxEntryId = taxEntryId,
            BankAccountId = bankAccountId,
            Amount = new Money(amount),
            Fine = new Money(fine),
            Interest = new Money(interest),
            PaymentDate = paymentDate,
            DarfNumber = darfNumber?.Trim(),
            ReceiptCode = receiptCode?.Trim(),
            Status = TaxPaymentStatus.Paid,
            Notes = notes?.Trim()
        };

        payment.AddDomainEvent(new TaxPaymentRegisteredEvent(payment.Id, taxEntryId, payment.TotalPaid, paymentDate));
        return payment;
    }

    public void Cancel(string reason)
    {
        if (Status == TaxPaymentStatus.Cancelled)
            throw new DomainException("Tax payment is already cancelled.");

        Status = TaxPaymentStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | Cancelled: {reason}";
        SetUpdatedAt();
    }
}
