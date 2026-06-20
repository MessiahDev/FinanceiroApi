using FinanceiroApi.Application.Commands.Auth.UpdateUserName;
using FinanceiroApi.Application.Commands.Auth.ChangePassword;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationContext _notifications;

    public UsersController(IMediator mediator, ICurrentUser currentUser, INotificationContext notifications)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _notifications = notifications;
    }

    [HttpPut("me/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateName([FromBody] UpdateNameRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateUserNameCommand(_currentUser.UserId, request.Name), ct);
        if (!result) return BadRequest(_notifications.Notifications);
        return NoContent();
    }

    [HttpPut("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ChangePasswordCommand(_currentUser.UserId, request.CurrentPassword, request.NewPassword), ct);
        if (!result) return BadRequest(_notifications.Notifications);
        return NoContent();
    }
}

public record UpdateNameRequest(string Name);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
