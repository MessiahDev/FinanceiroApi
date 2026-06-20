using FinanceiroApi.Domain.Entities.Base;
using FinanceiroApi.Domain.Events;
using FinanceiroApi.Domain.Exceptions;
using FinanceiroApi.Domain.Enums;
namespace FinanceiroApi.Domain.Entities;

public class User : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    protected User() { }
    public static User Create(string name, string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new DomainException("Password is required.");
        return new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            IsActive = true
        };
    }
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Name is required.");
        Name = name.Trim();
        SetUpdatedAt();
    }
    public void ChangeRole(UserRole newRole, Guid changedByUserId)
    {
        if (newRole == Role) return;
        var oldRole = Role;
        Role = newRole;
        SetUpdatedAt();
        AddDomainEvent(new UserRoleChangedEvent(Id, oldRole, newRole, changedByUserId));
    }
    public void Deactivate(Guid changedByUserId)
    {
        if (!IsActive) throw new DomainException("User is already inactive.");
        IsActive = false;
        SetUpdatedAt();
        AddDomainEvent(new UserDeactivatedEvent(Id, changedByUserId));
    }
    public void Activate(Guid changedByUserId)
    {
        if (IsActive) throw new DomainException("User is already active.");
        IsActive = true;
        SetUpdatedAt();
        AddDomainEvent(new UserActivatedEvent(Id, changedByUserId));
    }
    public void UpdatePasswordHash(string hash) { PasswordHash = hash; SetUpdatedAt(); }
}
