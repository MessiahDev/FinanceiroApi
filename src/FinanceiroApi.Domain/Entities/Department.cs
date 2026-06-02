using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Exceptions;

namespace FinanceiroApi.Domain.Entities;

public class Department : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string CostCenter { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private readonly List<Employee> _employees = [];
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    protected Department() { }

    public static Department Create(string name, string costCenter, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Department name is required.");

        if (string.IsNullOrWhiteSpace(costCenter))
            throw new DomainException("Cost center is required.");

        return new Department
        {
            Name = name.Trim(),
            CostCenter = costCenter.Trim().ToUpperInvariant(),
            Description = description?.Trim()
        };
    }

    public void Update(string name, string costCenter, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Department name is required.");

        Name = name.Trim();
        CostCenter = costCenter.Trim().ToUpperInvariant();
        Description = description?.Trim();
        SetUpdatedAt();
    }

    public void Deactivate() { IsActive = false; SetUpdatedAt(); }
    public void Activate() { IsActive = true; SetUpdatedAt(); }
}
