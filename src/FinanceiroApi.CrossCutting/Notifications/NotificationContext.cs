namespace FinanceiroApi.CrossCutting.Notifications;

public sealed class NotificationContext : INotificationContext
{
    private readonly List<Notification> _notifications = [];

    public IReadOnlyList<Notification> Notifications => _notifications;

    public bool HasErrors => _notifications.Any(n => n.Severity == NotificationSeverity.Error);
    public bool IsValid => !HasErrors;

    public bool HasNotifications => _notifications.Count > 0;

    public void AddError(string key, string message)
        => _notifications.Add(new Notification(key, message, NotificationSeverity.Error));

    public void AddWarning(string key, string message)
        => _notifications.Add(new Notification(key, message, NotificationSeverity.Warning));

    public void AddInfo(string key, string message)
        => _notifications.Add(new Notification(key, message, NotificationSeverity.Info));

    public void AddNotification(string key, string message)
        => AddError(key, message);

    public void AddRange(IEnumerable<Notification> notifications)
        => _notifications.AddRange(notifications);

    public void Clear() => _notifications.Clear();
}
