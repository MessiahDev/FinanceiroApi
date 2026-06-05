using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;
namespace FinanceiroApi.Domain.Entities;
public class AccountingPeriod : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public int Month { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public AccountingPeriodStatus Status { get; private set; }
    public IReadOnlyCollection<JournalEntry> JournalEntries => _journalEntries.AsReadOnly();
    private readonly List<JournalEntry> _journalEntries = new();
    protected AccountingPeriod() { }
    private AccountingPeriod(string name, int year, int month, DateRange period)
    {
        Name = name;
        Year = year;
        Month = month;
        Period = period;
        Status = AccountingPeriodStatus.Open;
        AddDomainEvent(new AccountingPeriodOpenedEvent(Id, Name, Year, Month));
    }
    public static AccountingPeriod Create(int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new DomainException("Ano do periodo contabil invalido.");
        if (month < 1 || month > 12)
            throw new DomainException("Mes do periodo contabil invalido.");
        var period = DateRange.ForMonth(year, month);
        var name = new DateTime(year, month, 1).ToString("MMMM/yyyy");
        return new AccountingPeriod(name, year, month, period);
    }
    public void Close()
    {
        if (Status == AccountingPeriodStatus.Closed)
            throw new DomainException("O periodo contabil ja esta fechado.");
        if (Status == AccountingPeriodStatus.Locked)
            throw new DomainException("O periodo contabil esta bloqueado e nao pode ser fechado diretamente.");
        Status = AccountingPeriodStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AccountingPeriodClosedEvent(Id, Name, Year, Month));
    }
    public void Lock()
    {
        if (Status != AccountingPeriodStatus.Closed)
            throw new DomainException("Apenas periodos fechados podem ser bloqueados.");
        Status = AccountingPeriodStatus.Locked;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new AccountingPeriodLockedEvent(Id, Name, Year, Month));
    }
    public void Reopen()
    {
        if (Status == AccountingPeriodStatus.Locked)
            throw new DomainException("Periodos bloqueados nao podem ser reabertos.");
        if (Status == AccountingPeriodStatus.Open)
            throw new DomainException("O periodo contabil ja esta aberto.");
        Status = AccountingPeriodStatus.Open;
        UpdatedAt = DateTime.UtcNow;
    }
    public bool IsOpen() => Status == AccountingPeriodStatus.Open;
    public bool AcceptsEntries() => Status == AccountingPeriodStatus.Open;
}
