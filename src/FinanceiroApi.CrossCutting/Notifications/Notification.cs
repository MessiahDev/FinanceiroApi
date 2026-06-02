namespace FinanceiroApi.CrossCutting.Notifications;

public enum NotificationSeverity
{
    Info = 0,
    Warning = 1,
    Error = 2
}

public sealed record Notification(
    string Key,
    string Message,
    NotificationSeverity Severity = NotificationSeverity.Error);
