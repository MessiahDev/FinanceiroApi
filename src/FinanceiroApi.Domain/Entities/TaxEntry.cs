using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class TaxEntry : AggregateRoot
{
    public TaxType TaxType { get; private set; }
    public string Description { get; private set; } = default!;
    public Money BaseAmount { get; private set; } = default!;
    public decimal Rate { get; private set; }
    public Money TaxAmount { get; private set; } = default!;
    public DateOnly Competence { get; private set; }
    public DateOnly DueDate { get; private set; }
    public TaxEntryStatus Status { get; private set; }
    public string? ReferenceDocument { get; private set; }
    public Guid? ReferenceDocumentId { get; private set; }
    public Guid? CostCenterId { get; private set; }
    public string? Notes { get; private set; }

    public CostCenter? CostCenter { get; private set; }
    public IReadOnlyList<TaxPayment> Payments => _payments.AsReadOnly();
    private readonly List<TaxPayment> _payments = [];

    protected TaxEntry() { }

    public static TaxEntry Create(
        TaxType taxType,
        string description,
        decimal baseAmount,
        decimal rate,
        DateOnly competence,
        DateOnly dueDate,
        Guid? costCenterId = null,
        string? referenceDocument = null,
        Guid? referenceDocumentId = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Description is required.");
        if (baseAmount <= 0)
            throw new DomainException("Base amount must be greater than zero.");
        if (rate < 0 || rate > 100)
            throw new DomainException("Tax rate must be between 0 and 100.");
        if (dueDate < competence)
            throw new DomainException("Due date cannot be before competence date.");

        var taxAmount = Math.Round(baseAmount * rate / 100, 2);

        var entry = new TaxEntry
        {
            TaxType = taxType,
            Description = description.Trim(),
            BaseAmount = new Money(baseAmount),
            Rate = rate,
            TaxAmount = new Money(taxAmount),
            Competence = competence,
            DueDate = dueDate,
            Status = TaxEntryStatus.Calculated,
            CostCenterId = costCenterId,
            ReferenceDocument = referenceDocument?.Trim(),
            ReferenceDocumentId = referenceDocumentId,
            Notes = notes?.Trim()
        };

        entry.AddDomainEvent(new TaxEntryCreatedEvent(entry.Id, taxType, entry.TaxAmount, dueDate));
        return entry;
    }

    public void MarkAsPaid()
    {
        if (Status == TaxEntryStatus.Paid)
            throw new DomainException("Tax entry is already paid.");
        if (Status == TaxEntryStatus.Cancelled)
            throw new DomainException("Cancelled tax entries cannot be paid.");

        Status = TaxEntryStatus.Paid;
        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status == TaxEntryStatus.Paid)
            throw new DomainException("Paid tax entries cannot be cancelled.");

        Status = TaxEntryStatus.Cancelled;
        Notes = string.IsNullOrWhiteSpace(Notes) ? reason : $"{Notes} | Cancelled: {reason}";
        SetUpdatedAt();
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes.Trim();
        SetUpdatedAt();
    }
}
