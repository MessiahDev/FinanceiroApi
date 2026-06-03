using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class BankAccount : AggregateRoot
{
    public string BankName { get; private set; } = default!;
    public string BankCode { get; private set; } = default!;
    public string Agency { get; private set; } = default!;
    public string AccountNumber { get; private set; } = default!;
    public BankAccountType AccountType { get; private set; }
    public string? PixKey { get; private set; }
    public Money Balance { get; private set; } = Money.Zero;
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }

    protected BankAccount() { }

    public static BankAccount Create(
        string bankName,
        string bankCode,
        string agency,
        string accountNumber,
        BankAccountType accountType,
        decimal initialBalance = 0,
        string? pixKey = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(bankName)) throw new DomainException("Bank name is required.");
        if (string.IsNullOrWhiteSpace(bankCode)) throw new DomainException("Bank code is required.");
        if (string.IsNullOrWhiteSpace(agency)) throw new DomainException("Agency is required.");
        if (string.IsNullOrWhiteSpace(accountNumber)) throw new DomainException("Account number is required.");

        var account = new BankAccount
        {
            BankName = bankName.Trim(),
            BankCode = bankCode.Trim(),
            Agency = agency.Trim(),
            AccountNumber = accountNumber.Trim(),
            AccountType = accountType,
            Balance = new Money(initialBalance),
            PixKey = pixKey?.Trim(),
            Description = description?.Trim()
        };

        account.AddDomainEvent(new BankAccountCreatedEvent(account.Id, bankName, accountNumber, account.Balance));
        return account;
    }

    public void Credit(Money amount, string description)
    {
        if (!IsActive) throw new DomainException("Cannot credit an inactive bank account.");
        if (amount.Amount <= 0) throw new DomainException("Credit amount must be greater than zero.");

        Balance = Balance + amount;
        SetUpdatedAt();
        AddDomainEvent(new BankAccountCreditedEvent(Id, amount, Balance, description));
    }

    public void Debit(Money amount, string description)
    {
        if (!IsActive) throw new DomainException("Cannot debit an inactive bank account.");
        if (amount.Amount <= 0) throw new DomainException("Debit amount must be greater than zero.");
        if (amount > Balance) throw new DomainException("Insufficient balance.");

        Balance = Balance - amount;
        SetUpdatedAt();
        AddDomainEvent(new BankAccountDebitedEvent(Id, amount, Balance, description));
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate() { IsActive = true; SetUpdatedAt(); }
}