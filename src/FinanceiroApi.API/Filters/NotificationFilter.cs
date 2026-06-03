using FinanceiroApi.CrossCutting.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinanceiroApi.API.Filters;

public class NotificationFilter : IAsyncActionFilter
{
    private readonly INotificationContext _notificationContext;

    public NotificationFilter(INotificationContext notificationContext)
        => _notificationContext = notificationContext;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        if (_notificationContext.HasNotifications)
        {
            executedContext.Result = new UnprocessableEntityObjectResult(
                new { errors = _notificationContext.Notifications.Select(n => n.Message) });
        }
    }
}
