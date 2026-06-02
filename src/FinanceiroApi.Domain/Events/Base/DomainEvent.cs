using MediatR;

namespace FinanceiroApi.Domain.Events.Base;

public abstract class DomainEvent : INotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}