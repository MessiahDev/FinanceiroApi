namespace FinanceiroApi.CrossCutting.Notifications;

public interface INotificationContext
{
    IReadOnlyList<Notification> Notifications { get; }
    bool HasErrors { get; }
    bool IsValid { get; }
    bool HasNotifications { get; }

    void AddError(string key, string message);
    void AddWarning(string key, string message);
    void AddInfo(string key, string message);
    void AddNotification(string key, string message);
}
