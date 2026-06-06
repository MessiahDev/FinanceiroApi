using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events.Base;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Events;

public sealed class TaxEntryCreatedEvent(
    Guid taxEntryId,
    TaxType taxType,
    Money taxAmount,
    DateOnly dueDate) : DomainEvent
{
    public Guid TaxEntryId { get; } = taxEntryId;
    public TaxType TaxType { get; } = taxType;
    public Money TaxAmount { get; } = taxAmount;
    public DateOnly DueDate { get; } = dueDate;
}

public sealed class TaxEntryCancelledEvent(
    Guid taxEntryId,
    TaxType taxType,
    string reason) : DomainEvent
{
    public Guid TaxEntryId { get; } = taxEntryId;
    public TaxType TaxType { get; } = taxType;
    public string Reason { get; } = reason;
}

public sealed class TaxPaymentRegisteredEvent(
    Guid taxPaymentId,
    Guid taxEntryId,
    Money totalPaid,
    DateOnly paymentDate) : DomainEvent
{
    public Guid TaxPaymentId { get; } = taxPaymentId;
    public Guid TaxEntryId { get; } = taxEntryId;
    public Money TotalPaid { get; } = totalPaid;
    public DateOnly PaymentDate { get; } = paymentDate;
}

public sealed class TaxPaymentCancelledEvent(
    Guid taxPaymentId,
    Guid taxEntryId,
    string reason) : DomainEvent
{
    public Guid TaxPaymentId { get; } = taxPaymentId;
    public Guid TaxEntryId { get; } = taxEntryId;
    public string Reason { get; } = reason;
}

public sealed class BankStatementImportedEvent(
    Guid bankStatementId,
    Guid bankAccountId,
    DateOnly periodStart,
    DateOnly periodEnd) : DomainEvent
{
    public Guid BankStatementId { get; } = bankStatementId;
    public Guid BankAccountId { get; } = bankAccountId;
    public DateOnly PeriodStart { get; } = periodStart;
    public DateOnly PeriodEnd { get; } = periodEnd;
}

public sealed class BankStatementCancelledEvent(
    Guid bankStatementId,
    Guid bankAccountId,
    string reason) : DomainEvent
{
    public Guid BankStatementId { get; } = bankStatementId;
    public Guid BankAccountId { get; } = bankAccountId;
    public string Reason { get; } = reason;
}

public sealed class BankReconciliationCreatedEvent(
    Guid bankReconciliationId,
    Guid bankAccountId,
    DateOnly periodStart,
    DateOnly periodEnd) : DomainEvent
{
    public Guid BankReconciliationId { get; } = bankReconciliationId;
    public Guid BankAccountId { get; } = bankAccountId;
    public DateOnly PeriodStart { get; } = periodStart;
    public DateOnly PeriodEnd { get; } = periodEnd;
}

public sealed class BankReconciliationCompletedEvent(
    Guid bankReconciliationId,
    Guid bankAccountId,
    decimal difference) : DomainEvent
{
    public Guid BankReconciliationId { get; } = bankReconciliationId;
    public Guid BankAccountId { get; } = bankAccountId;
    public decimal Difference { get; } = difference;
}

public sealed class BankReconciliationCancelledEvent(
    Guid bankReconciliationId,
    Guid bankAccountId,
    string reason) : DomainEvent
{
    public Guid BankReconciliationId { get; } = bankReconciliationId;
    public Guid BankAccountId { get; } = bankAccountId;
    public string Reason { get; } = reason;
}
