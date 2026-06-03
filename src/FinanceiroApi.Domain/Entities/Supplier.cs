using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class Supplier : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string TaxId { get; private set; } = default!;
    public PersonType PersonType { get; private set; }
    public Email Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? ContactName { get; private set; }
    public SupplierStatus Status { get; private set; }

    public string? BankName { get; private set; }
    public string? BankAgency { get; private set; }
    public string? BankAccount { get; private set; }
    public string? PixKey { get; private set; }

    private readonly List<AccountPayable> _payables = [];
    public IReadOnlyCollection<AccountPayable> Payables => _payables.AsReadOnly();

    protected Supplier() { }

    public static Supplier Create(
        string name,
        string taxId,
        PersonType personType,
        string email,
        string? phone = null,
        string? contactName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Supplier name is required.");
        if (string.IsNullOrWhiteSpace(taxId))
            throw new DomainException("Tax ID is required.");

        var supplier = new Supplier
        {
            Name = name.Trim(),
            TaxId = taxId.Replace(".", "").Replace("/", "").Replace("-", "").Trim(),
            PersonType = personType,
            Email = new Email(email),
            Phone = phone?.Trim(),
            ContactName = contactName?.Trim(),
            Status = SupplierStatus.Active
        };

        supplier.AddDomainEvent(new SupplierCreatedEvent(supplier.Id, supplier.Name, supplier.TaxId));
        return supplier;
    }

    public void UpdateBankingInfo(string bankName, string agency, string account, string? pixKey = null)
    {
        if (string.IsNullOrWhiteSpace(bankName)) throw new DomainException("Bank name is required.");
        if (string.IsNullOrWhiteSpace(agency)) throw new DomainException("Agency is required.");
        if (string.IsNullOrWhiteSpace(account)) throw new DomainException("Account is required.");

        BankName = bankName.Trim();
        BankAgency = agency.Trim();
        BankAccount = account.Trim();
        PixKey = pixKey?.Trim();
        SetUpdatedAt();
    }

    public void Update(string name, string email, string? phone, string? contactName)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Supplier name is required.");

        Name = name.Trim();
        Email = new Email(email);
        Phone = phone?.Trim();
        ContactName = contactName?.Trim();
        SetUpdatedAt();
    }

    public void Block(string reason)
    {
        if (Status == SupplierStatus.Blocked)
            throw new DomainException("Supplier is already blocked.");

        Status = SupplierStatus.Blocked;
        SetUpdatedAt();
        AddDomainEvent(new SupplierBlockedEvent(Id, Name, reason));
    }

    public void Activate()
    {
        Status = SupplierStatus.Active;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        Status = SupplierStatus.Inactive;
        SetUpdatedAt();
    }
}