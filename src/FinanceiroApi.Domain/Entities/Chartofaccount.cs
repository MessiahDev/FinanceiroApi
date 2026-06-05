using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Entities;

public class ChartOfAccount : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public AccountType AccountType { get; private set; }
    public AccountNature AccountNature { get; private set; }
    public bool AcceptsEntries { get; private set; }
    public bool IsActive { get; private set; }

    public Guid? ParentAccountId { get; private set; }
    public ChartOfAccount? ParentAccount { get; private set; }
    public IReadOnlyCollection<ChartOfAccount> ChildAccounts => _childAccounts.AsReadOnly();
    private readonly List<ChartOfAccount> _childAccounts = new();

    public IReadOnlyCollection<JournalEntryLine> JournalEntryLines => _journalEntryLines.AsReadOnly();
    private readonly List<JournalEntryLine> _journalEntryLines = new();

    protected ChartOfAccount() { }

    private ChartOfAccount(
        string code,
        string name,
        string? description,
        AccountType accountType,
        AccountNature accountNature,
        bool acceptsEntries,
        Guid? parentAccountId)
    {
        Code = code;
        Name = name;
        Description = description;
        AccountType = accountType;
        AccountNature = accountNature;
        AcceptsEntries = acceptsEntries;
        ParentAccountId = parentAccountId;
        IsActive = true;

        AddDomainEvent(new ChartOfAccountCreatedEvent(Id, Code, Name, AccountType));
    }

    public static ChartOfAccount Create(
        string code,
        string name,
        string? description,
        AccountType accountType,
        AccountNature accountNature,
        bool acceptsEntries,
        Guid? parentAccountId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("O código da conta é obrigatório.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da conta é obrigatório.");

        return new ChartOfAccount(code, name, description, accountType, accountNature, acceptsEntries, parentAccountId);
    }

    public void Update(string name, string? description, bool acceptsEntries)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("O nome da conta é obrigatório.");

        Name = name;
        Description = description;
        AcceptsEntries = acceptsEntries;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (_journalEntryLines.Any())
            throw new DomainException("Não é possível desativar uma conta que possui lançamentos.");

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ChartOfAccountDeactivatedEvent(Id, Code, Name));
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}