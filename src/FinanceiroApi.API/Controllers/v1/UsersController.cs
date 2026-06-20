using FinanceiroApi.Application.Commands.Auth.UpdateUserName;
using FinanceiroApi.Application.Commands.Auth.ChangePassword;
using FinanceiroApi.Application.Commands.Users.ChangeUserRole;
using FinanceiroApi.Application.Commands.Users.ActivateUser;
using FinanceiroApi.Application.Commands.Users.DeactivateUser;
using FinanceiroApi.Application.Queries.Users.GetAllUsers;
using FinanceiroApi.Application.Queries.Users.GetUserAuditLog;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Services;
using FinanceiroApi.Domain.Enums;
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

    [HttpGet]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllUsersQuery(), ct);
        return Ok(result);
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ChangeUserRoleCommand(id, request.Role, _currentUser.UserId), ct);
        if (!result) return BadRequest(_notifications.Notifications);
        return NoContent();
    }

    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ActivateUserCommand(id, _currentUser.UserId), ct);
        if (!result) return BadRequest(_notifications.Notifications);
        return NoContent();
    }

    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeactivateUserCommand(id, _currentUser.UserId), ct);
        if (!result) return BadRequest(_notifications.Notifications);
        return NoContent();
    }

    [HttpGet("audit-log")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLog([FromQuery] Guid? userId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUserAuditLogQuery(userId), ct);
        return Ok(result);
    }
}

public record UpdateNameRequest(string Name);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ChangeRoleRequest(UserRole Role);
