using FinanceiroApi.CrossCutting.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FinanceiroApi.API.Filters;

public class NotificationFilter : IAsyncResultFilter
{
    private readonly INotificationContext _notificationContext;

    public NotificationFilter(INotificationContext notificationContext)
        => _notificationContext = notificationContext;

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (_notificationContext.HasNotifications && context.Result is not BadRequestObjectResult)
        {
            context.Result = new UnprocessableEntityObjectResult(_notificationContext.Notifications);
            return;
        }

        await next();
    }
}
