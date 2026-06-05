using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("FinanceiroApi.Infrastructure")]

namespace FinanceiroApi.Domain.Entities.Base;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    internal void SetCreatedAt(DateTime date) => CreatedAt = date;
    internal void SetUpdatedAt(DateTime date) => UpdatedAt = date;
    protected void SetUpdatedAt() => UpdatedAt = DateTime.UtcNow;

    public void SoftDelete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
