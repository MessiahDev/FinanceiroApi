using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Enums;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.ValueObjects;

namespace FinanceiroApi.Domain.Entities;

public class CostCenter : AggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid? ParentId { get; private set; }
    public Money AnnualBudget { get; private set; } = Money.Zero;
    public CostCenterStatus Status { get; private set; }
    public Guid? ManagerId { get; private set; }

    public CostCenter? Parent { get; private set; }
    public Employee? Manager { get; private set; }

    private readonly List<CostCenter> _children = [];
    public IReadOnlyCollection<CostCenter> Children => _children.AsReadOnly();

    protected CostCenter() { }

    public static CostCenter Create(
        string code,
        string name,
        decimal annualBudget,
        Guid? parentId = null,
        Guid? managerId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Cost center code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Cost center name is required.");
        if (annualBudget < 0) throw new DomainException("Annual budget cannot be negative.");

        return new CostCenter
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ParentId = parentId,
            AnnualBudget = new Money(annualBudget),
            ManagerId = managerId,
            Status = CostCenterStatus.Active
        };
    }

    public void UpdateBudget(decimal newBudget)
    {
        if (newBudget < 0) throw new DomainException("Annual budget cannot be negative.");
        AnnualBudget = new Money(newBudget);
        SetUpdatedAt();
    }

    public void Update(string code, string name, string? description, Guid? managerId)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Cost center code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Cost center name is required.");

        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = description?.Trim();
        ManagerId = managerId;
        SetUpdatedAt();
    }

    public void Deactivate() { Status = CostCenterStatus.Inactive; SetUpdatedAt(); }
    public void Activate() { Status = CostCenterStatus.Active; SetUpdatedAt(); }
}