using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class BudgetItem : BaseEntity
{
    public Guid BudgetId { get; private set; }
    public Guid CostCenterId { get; private set; }
    public string Category { get; private set; } = default!;
    public Money PlannedAmount { get; private set; } = default!;
    public Money RealizedAmount { get; private set; } = Money.Zero;
    public Money Variance => PlannedAmount - RealizedAmount;
    public bool IsOverBudget => RealizedAmount.Amount > PlannedAmount.Amount;

    public CostCenter? CostCenter { get; private set; }

    protected BudgetItem() { }

    internal static BudgetItem Create(Guid budgetId, Guid costCenterId, string category, decimal plannedAmount)
    {
        if (string.IsNullOrWhiteSpace(category)) throw new DomainException("Category is required.");
        if (plannedAmount <= 0) throw new DomainException("Planned amount must be greater than zero.");

        return new BudgetItem
        {
            BudgetId = budgetId,
            CostCenterId = costCenterId,
            Category = category.Trim(),
            PlannedAmount = new Money(plannedAmount)
        };
    }

    internal void RegisterRealization(decimal amount)
    {
        if (amount <= 0) throw new DomainException("Realized amount must be greater than zero.");
        RealizedAmount = RealizedAmount + new Money(amount);
        SetUpdatedAt();
    }
}

public class Budget : AggregateRoot
{
    public int Year { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public BudgetStatus Status { get; private set; }
    public Money TotalPlanned { get; private set; } = Money.Zero;
    public Money TotalRealized { get; private set; } = Money.Zero;
    public Money Variance => TotalPlanned - TotalRealized;
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }

    private readonly List<BudgetItem> _items = [];
    public IReadOnlyCollection<BudgetItem> Items => _items.AsReadOnly();

    protected Budget() { }

    public static Budget Create(int year, string name, string? description = null)
    {
        if (year < 2000 || year > 2100) throw new DomainException("Invalid year.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Budget name is required.");

        return new Budget
        {
            Year = year,
            Name = name.Trim(),
            Description = description?.Trim(),
            Status = BudgetStatus.Draft
        };
    }

    public void AddItem(Guid costCenterId, string category, decimal plannedAmount)
    {
        if (Status != BudgetStatus.Draft)
            throw new DomainException("Items can only be added to Draft budgets.");

        if (_items.Any(i => i.CostCenterId == costCenterId && i.Category == category.Trim()))
            throw new DomainException("This cost center/category combination already exists.");

        var item = BudgetItem.Create(Id, costCenterId, category, plannedAmount);
        _items.Add(item);
        RecalculateTotals();
    }

    public void RegisterRealization(Guid costCenterId, string category, decimal amount)
    {
        if (Status != BudgetStatus.Approved)
            throw new DomainException("Realizations can only be registered in Approved budgets.");

        var item = _items.FirstOrDefault(i =>
            i.CostCenterId == costCenterId && i.Category == category.Trim())
            ?? throw new DomainException("Budget item not found for this cost center/category.");

        item.RegisterRealization(amount);
        RecalculateTotals();
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != BudgetStatus.Draft)
            throw new DomainException("Only Draft budgets can be approved.");
        if (!_items.Any())
            throw new DomainException("Cannot approve an empty budget.");

        Status = BudgetStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedBy = approvedBy;
        SetUpdatedAt();

        AddDomainEvent(new BudgetApprovedEvent(Id, Year, TotalPlanned, approvedBy));
    }

    public void Close()
    {
        if (Status != BudgetStatus.Approved)
            throw new DomainException("Only Approved budgets can be closed.");

        Status = BudgetStatus.Closed;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == BudgetStatus.Closed)
            throw new DomainException("Closed budgets cannot be cancelled.");

        Status = BudgetStatus.Cancelled;
        SetUpdatedAt();
    }

    private void RecalculateTotals()
    {
        TotalPlanned = _items.Aggregate(Money.Zero, (acc, i) => acc + i.PlannedAmount);
        TotalRealized = _items.Aggregate(Money.Zero, (acc, i) => acc + i.RealizedAmount);
    }
}