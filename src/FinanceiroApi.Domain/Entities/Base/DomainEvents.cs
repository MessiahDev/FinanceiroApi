using FinanceiroApi.Domain.Events.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Events;

public sealed class EmployeeCreatedEvent(Guid employeeId, string fullName, Email email) : DomainEvent
{
    public Guid EmployeeId { get; } = employeeId;
    public string FullName { get; } = fullName;
    public string Email { get; } = email.Value;
}

public sealed class EmployeeSalaryUpdatedEvent(Guid employeeId, Money oldSalary, Money newSalary) : DomainEvent
{
    public Guid EmployeeId { get; } = employeeId;
    public Money OldSalary { get; } = oldSalary;
    public Money NewSalary { get; } = newSalary;
}

public sealed class EmployeeTerminatedEvent(Guid employeeId, string fullName, DateOnly terminationDate) : DomainEvent
{
    public Guid EmployeeId { get; } = employeeId;
    public string FullName { get; } = fullName;
    public DateOnly TerminationDate { get; } = terminationDate;
}

public sealed class PayrollProcessedEvent(Guid payrollId, DateRange period, Money totalNet) : DomainEvent
{
    public Guid PayrollId { get; } = payrollId;
    public DateRange Period { get; } = period;
    public Money TotalNet { get; } = totalNet;
}

public sealed class PayrollPaidEvent(Guid payrollId, DateRange period, Money totalNet, DateTime paidAt) : DomainEvent
{
    public Guid PayrollId { get; } = payrollId;
    public DateRange Period { get; } = period;
    public Money TotalNet { get; } = totalNet;
    public DateTime PaidAt { get; } = paidAt;
}

public sealed class PayrollCancelledEvent(Guid payrollId, DateRange period, string reason) : DomainEvent
{
    public Guid PayrollId { get; } = payrollId;
    public DateRange Period { get; } = period;
    public string Reason { get; } = reason;
}

public sealed class TransactionCreatedEvent(Guid transactionId, Money amount, TransactionType type, TransactionCategory category) : DomainEvent
{
    public Guid TransactionId { get; } = transactionId;
    public Money Amount { get; } = amount;
    public TransactionType Type { get; } = type;
    public TransactionCategory Category { get; } = category;
}

public sealed class TransactionConfirmedEvent(Guid transactionId, Money amount, TransactionType type, TransactionCategory category, string description) : DomainEvent
{
    public Guid TransactionId { get; } = transactionId;
    public Money Amount { get; } = amount;
    public TransactionType Type { get; } = type;
    public TransactionCategory Category { get; } = category;
    public string Description { get; } = description;
}

public sealed class SupplierCreatedEvent(Guid supplierId, string name, string taxId) : DomainEvent
{
    public Guid SupplierId { get; } = supplierId;
    public string Name { get; } = name;
    public string TaxId { get; } = taxId;
}

public sealed class SupplierBlockedEvent(Guid supplierId, string name, string reason) : DomainEvent
{
    public Guid SupplierId { get; } = supplierId;
    public string Name { get; } = name;
    public string Reason { get; } = reason;
}

public sealed class CustomerCreatedEvent(Guid customerId, string name, string taxId) : DomainEvent
{
    public Guid CustomerId { get; } = customerId;
    public string Name { get; } = name;
    public string TaxId { get; } = taxId;
}

public sealed class CustomerBlockedEvent(Guid customerId, string name, string reason) : DomainEvent
{
    public Guid CustomerId { get; } = customerId;
    public string Name { get; } = name;
    public string Reason { get; } = reason;
}

public sealed class AccountPayableCreatedEvent(Guid id, Guid supplierId, Money amount, DateOnly dueDate) : DomainEvent
{
    public new Guid Id { get; } = id;
    public Guid SupplierId { get; } = supplierId;
    public Money Amount { get; } = amount;
    public DateOnly DueDate { get; } = dueDate;
}

public sealed class AccountPayablePaidEvent(Guid id, Guid supplierId, Money paidAmount, AccountPayableStatus status) : DomainEvent
{
    public new Guid Id { get; } = id;
    public Guid SupplierId { get; } = supplierId;
    public Money PaidAmount { get; } = paidAmount;
    public AccountPayableStatus Status { get; } = status;
}

public sealed class AccountReceivableCreatedEvent(Guid id, Guid customerId, Money amount, DateOnly dueDate) : DomainEvent
{
    public new Guid Id { get; } = id;
    public Guid CustomerId { get; } = customerId;
    public Money Amount { get; } = amount;
    public DateOnly DueDate { get; } = dueDate;
}

public sealed class AccountReceivableReceivedEvent(Guid id, Guid customerId, Money receivedAmount, AccountReceivableStatus status) : DomainEvent
{
    public new Guid Id { get; } = id;
    public Guid CustomerId { get; } = customerId;
    public Money ReceivedAmount { get; } = receivedAmount;
    public AccountReceivableStatus Status { get; } = status;
}

public sealed class BankAccountCreatedEvent(Guid id, string bankName, string accountNumber, Money initialBalance) : DomainEvent
{
    public new Guid Id { get; } = id;
    public string BankName { get; } = bankName;
    public string AccountNumber { get; } = accountNumber;
    public Money InitialBalance { get; } = initialBalance;
}

public sealed class BankAccountCreditedEvent(Guid id, Money amount, Money newBalance, string description) : DomainEvent
{
    public new Guid Id { get; } = id;
    public Money Amount { get; } = amount;
    public Money NewBalance { get; } = newBalance;
    public string Description { get; } = description;
}

public sealed class BankAccountDebitedEvent(Guid id, Money amount, Money newBalance, string description) : DomainEvent
{
    public new Guid Id { get; } = id;
    public Money Amount { get; } = amount;
    public Money NewBalance { get; } = newBalance;
    public string Description { get; } = description;
}

public sealed class BudgetApprovedEvent(Guid budgetId, int year, Money totalPlanned, Guid approvedBy) : DomainEvent
{
    public Guid BudgetId { get; } = budgetId;
    public int Year { get; } = year;
    public Money TotalPlanned { get; } = totalPlanned;
    public Guid ApprovedBy { get; } = approvedBy;
}

