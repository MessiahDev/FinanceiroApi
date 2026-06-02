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
