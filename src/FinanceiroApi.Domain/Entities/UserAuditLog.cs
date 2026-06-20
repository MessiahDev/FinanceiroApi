using FinanceiroApi.Domain.Entities.Base;

namespace FinanceiroApi.Domain.Entities;

public class UserAuditLog : BaseEntity
{
    public Guid TargetUserId { get; private set; }
    public Guid ChangedByUserId { get; private set; }
    public string Action { get; private set; } = default!;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }

    public User? TargetUser { get; private set; }
    public User? ChangedByUser { get; private set; }

    protected UserAuditLog() { }

    public static UserAuditLog Create(
        Guid targetUserId,
        Guid changedByUserId,
        string action,
        string? oldValue,
        string? newValue)
    {
        return new UserAuditLog
        {
            TargetUserId = targetUserId,
            ChangedByUserId = changedByUserId,
            Action = action,
            OldValue = oldValue,
            NewValue = newValue
        };
    }
}
