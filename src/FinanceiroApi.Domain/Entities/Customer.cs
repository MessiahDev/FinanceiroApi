using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class Customer : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string TaxId { get; private set; } = default!;
    public PersonType PersonType { get; private set; }
    public Email Email { get; private set; } = default!;
    public string? Phone { get; private set; }
    public string? ContactName { get; private set; }
    public CustomerStatus Status { get; private set; }
    public Money CreditLimit { get; private set; } = Money.Zero;

    private readonly List<AccountReceivable> _receivables = [];
    public IReadOnlyCollection<AccountReceivable> Receivables => _receivables.AsReadOnly();

    protected Customer() { }

    public static Customer Create(
        string name,
        string taxId,
        PersonType personType,
        string email,
        string? phone = null,
        string? contactName = null,
        decimal creditLimit = 0)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Customer name is required.");
        if (string.IsNullOrWhiteSpace(taxId))
            throw new DomainException("Tax ID is required.");

        var customer = new Customer
        {
            Name = name.Trim(),
            TaxId = taxId.Replace(".", "").Replace("/", "").Replace("-", "").Trim(),
            PersonType = personType,
            Email = new Email(email),
            Phone = phone?.Trim(),
            ContactName = contactName?.Trim(),
            Status = CustomerStatus.Active,
            CreditLimit = new Money(creditLimit)
        };

        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.Name, customer.TaxId));
        return customer;
    }

    public void UpdateCreditLimit(decimal newLimit)
    {
        CreditLimit = new Money(newLimit);
        SetUpdatedAt();
    }

    public void Update(string name, string email, string? phone, string? contactName)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Customer name is required.");

        Name = name.Trim();
        Email = new Email(email);
        Phone = phone?.Trim();
        ContactName = contactName?.Trim();
        SetUpdatedAt();
    }

    public void Block(string reason)
    {
        if (Status == CustomerStatus.Blocked)
            throw new DomainException("Customer is already blocked.");

        Status = CustomerStatus.Blocked;
        SetUpdatedAt();
        AddDomainEvent(new CustomerBlockedEvent(Id, Name, reason));
    }

    public void Activate() { Status = CustomerStatus.Active; SetUpdatedAt(); }
    public void Deactivate() { Status = CustomerStatus.Inactive; SetUpdatedAt(); }
}