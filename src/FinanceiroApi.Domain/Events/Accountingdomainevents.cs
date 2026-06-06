using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events.Base;

namespace FinanceiroApi.Domain.Events;

public sealed class ChartOfAccountCreatedEvent(
    Guid accountId,
    string code,
    string name,
    AccountType accountType) : DomainEvent
{
    public Guid AccountId { get; } = accountId;
    public string Code { get; } = code;
    public string Name { get; } = name;
    public AccountType AccountType { get; } = accountType;
}

public sealed class ChartOfAccountDeactivatedEvent(
    Guid accountId,
    string code,
    string name) : DomainEvent
{
    public Guid AccountId { get; } = accountId;
    public string Code { get; } = code;
    public string Name { get; } = name;
}

public sealed class JournalEntryPostedEvent(
    Guid journalEntryId,
    string entryNumber,
    DateTime entryDate,
    decimal totalAmount) : DomainEvent
{
    public Guid JournalEntryId { get; } = journalEntryId;
    public string EntryNumber { get; } = entryNumber;
    public DateTime EntryDate { get; } = entryDate;
    public decimal TotalAmount { get; } = totalAmount;
}

public sealed class JournalEntryReversedEvent(
    Guid journalEntryId,
    string originalEntryNumber,
    string reversalDescription,
    Guid reversedByUserId) : DomainEvent
{
    public Guid JournalEntryId { get; } = journalEntryId;
    public string OriginalEntryNumber { get; } = originalEntryNumber;
    public string ReversalDescription { get; } = reversalDescription;
    public Guid ReversedByUserId { get; } = reversedByUserId;
}

public sealed class AccountingPeriodOpenedEvent(
    Guid periodId,
    string periodName,
    int year,
    int month) : DomainEvent
{
    public Guid PeriodId { get; } = periodId;
    public string PeriodName { get; } = periodName;
    public int Year { get; } = year;
    public int Month { get; } = month;
}

public sealed class AccountingPeriodClosedEvent(
    Guid periodId,
    string periodName,
    int year,
    int month) : DomainEvent
{
    public Guid PeriodId { get; } = periodId;
    public string PeriodName { get; } = periodName;
    public int Year { get; } = year;
    public int Month { get; } = month;
}

public sealed class AccountingPeriodLockedEvent(
    Guid periodId,
    string periodName,
    int year,
    int month) : DomainEvent
{
    public Guid PeriodId { get; } = periodId;
    public string PeriodName { get; } = periodName;
    public int Year { get; } = year;
    public int Month { get; } = month;
}
