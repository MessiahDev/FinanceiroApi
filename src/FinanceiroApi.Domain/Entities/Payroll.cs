using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class PayrollItem : BaseEntity
{
    public Guid PayrollId { get; private set; }
    public Guid EmployeeId { get; private set; }
    public Money GrossSalary { get; private set; } = default!;
    public Money InssDiscount { get; private set; } = default!;
    public Money IrpfDiscount { get; private set; } = default!;
    public Money OtherDiscounts { get; private set; } = default!;
    public Money NetSalary { get; private set; } = default!;

    public Employee? Employee { get; private set; }

    protected PayrollItem() { }

    internal static PayrollItem Create(
        Guid payrollId,
        Guid employeeId,
        Money grossSalary,
        Money inss,
        Money irpf,
        Money otherDiscounts)
    {
        var net = grossSalary - inss - irpf - otherDiscounts;

        return new PayrollItem
        {
            PayrollId = payrollId,
            EmployeeId = employeeId,
            GrossSalary = grossSalary,
            InssDiscount = inss,
            IrpfDiscount = irpf,
            OtherDiscounts = otherDiscounts,
            NetSalary = net
        };
    }
}

public class Payroll : AggregateRoot
{
    public DateRange Period { get; private set; } = default!;
    public PayrollStatus Status { get; private set; }
    public Money TotalGross { get; private set; } = Money.Zero;
    public Money TotalDiscounts { get; private set; } = Money.Zero;
    public Money TotalNet { get; private set; } = Money.Zero;
    public string? Notes { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private readonly List<PayrollItem> _items = [];
    public IReadOnlyCollection<PayrollItem> Items => _items.AsReadOnly();

    protected Payroll() { }

    public static Payroll Create(int year, int month, string? notes = null)
    {
        if (year < 2000 || year > 2100) throw new DomainException("Invalid year.");
        if (month < 1 || month > 12) throw new DomainException("Invalid month.");

        return new Payroll
        {
            Period = DateRange.ForMonth(year, month),
            Status = PayrollStatus.Draft,
            Notes = notes
        };
    }

    public void AddItem(Guid employeeId, Money grossSalary, Money inss, Money irpf, Money otherDiscounts)
    {
        EnsureDraft();

        if (_items.Any(i => i.EmployeeId == employeeId))
            throw new DomainException("Employee already added to this payroll.");

        var item = PayrollItem.Create(Id, employeeId, grossSalary, inss, irpf, otherDiscounts);
        _items.Add(item);
        RecalculateTotals();
    }

    public void Process()
    {
        EnsureDraft();
        if (!_items.Any()) throw new DomainException("Cannot process an empty payroll.");

        Status = PayrollStatus.Processing;
        ProcessedAt = DateTime.UtcNow;
        SetUpdatedAt();

        AddDomainEvent(new PayrollProcessedEvent(Id, Period, TotalNet));
    }

    public void Approve()
    {
        if (Status != PayrollStatus.Processing)
            throw new DomainException("Only payrolls in Processing status can be approved.");

        Status = PayrollStatus.Approved;
        SetUpdatedAt();
    }

    public void MarkAsPaid()
    {
        if (Status != PayrollStatus.Approved)
            throw new DomainException("Only approved payrolls can be marked as paid.");

        Status = PayrollStatus.Paid;
        PaidAt = DateTime.UtcNow;
        SetUpdatedAt();

        AddDomainEvent(new PayrollPaidEvent(Id, Period, TotalNet, PaidAt.Value));
    }

    public void Cancel(string reason)
    {
        if (Status == PayrollStatus.Paid)
            throw new DomainException("Paid payrolls cannot be cancelled.");

        Status = PayrollStatus.Cancelled;
        Notes = reason;
        SetUpdatedAt();

        AddDomainEvent(new PayrollCancelledEvent(Id, Period, reason));
    }

    private void EnsureDraft()
    {
        if (Status != PayrollStatus.Draft)
            throw new DomainException($"Payroll is not in Draft status. Current: {Status}.");
    }

    private void RecalculateTotals()
    {
        TotalGross = _items.Aggregate(Money.Zero, (acc, i) => acc + i.GrossSalary);
        TotalDiscounts = _items.Aggregate(Money.Zero, (acc, i) => acc + i.InssDiscount + i.IrpfDiscount + i.OtherDiscounts);
        TotalNet = _items.Aggregate(Money.Zero, (acc, i) => acc + i.NetSalary);
    }
}
