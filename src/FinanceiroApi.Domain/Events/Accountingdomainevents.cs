using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events.Base;

namespace FinanceiroApi.Domain.Events;

public sealed class ChartOfAccountCreatedEvent : DomainEvent
{
    public Guid AccountId { get; }
    public string Code { get; }
    public string Name { get; }
    public AccountType AccountType { get; }
    public ChartOfAccountCreatedEvent(Guid accountId, string code, string name, AccountType accountType)
    { AccountId = accountId; Code = code; Name = name; AccountType = accountType; }
}

public sealed class ChartOfAccountDeactivatedEvent : DomainEvent
{
    public Guid AccountId { get; }
    public string Code { get; }
    public string Name { get; }
    public ChartOfAccountDeactivatedEvent(Guid accountId, string code, string name)
    { AccountId = accountId; Code = code; Name = name; }
}

public sealed class JournalEntryPostedEvent : DomainEvent
{
    public Guid JournalEntryId { get; }
    public string EntryNumber { get; }
    public DateTime EntryDate { get; }
    public decimal TotalAmount { get; }
    public JournalEntryPostedEvent(Guid journalEntryId, string entryNumber, DateTime entryDate, decimal totalAmount)
    { JournalEntryId = journalEntryId; EntryNumber = entryNumber; EntryDate = entryDate; TotalAmount = totalAmount; }
}

public sealed class JournalEntryReversedEvent : DomainEvent
{
    public Guid JournalEntryId { get; }
    public string OriginalEntryNumber { get; }
    public string ReversalDescription { get; }
    public Guid ReversedByUserId { get; }
    public JournalEntryReversedEvent(Guid journalEntryId, string originalEntryNumber, string reversalDescription, Guid reversedByUserId)
    { JournalEntryId = journalEntryId; OriginalEntryNumber = originalEntryNumber; ReversalDescription = reversalDescription; ReversedByUserId = reversedByUserId; }
}

public sealed class AccountingPeriodOpenedEvent : DomainEvent
{
    public Guid PeriodId { get; }
    public string PeriodName { get; }
    public int Year { get; }
    public int Month { get; }
    public AccountingPeriodOpenedEvent(Guid periodId, string periodName, int year, int month)
    { PeriodId = periodId; PeriodName = periodName; Year = year; Month = month; }
}

public sealed class AccountingPeriodClosedEvent : DomainEvent
{
    public Guid PeriodId { get; }
    public string PeriodName { get; }
    public int Year { get; }
    public int Month { get; }
    public AccountingPeriodClosedEvent(Guid periodId, string periodName, int year, int month)
    { PeriodId = periodId; PeriodName = periodName; Year = year; Month = month; }
}

public sealed class AccountingPeriodLockedEvent : DomainEvent
{
    public Guid PeriodId { get; }
    public string PeriodName { get; }
    public int Year { get; }
    public int Month { get; }
    public AccountingPeriodLockedEvent(Guid periodId, string periodName, int year, int month)
    { PeriodId = periodId; PeriodName = periodName; Year = year; Month = month; }
}
