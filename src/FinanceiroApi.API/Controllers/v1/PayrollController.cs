using FinanceiroApi.Application.Commands.Payroll.CancelPayroll;
using FinanceiroApi.Application.Commands.Payroll.ProcessPayroll;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.Payroll.GetPayrollById;
using FinanceiroApi.Application.Queries.Payroll.GetPayrollHistory;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Pagination;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class PayrollController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public PayrollController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PayrollResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetPayrollHistoryQuery(page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PayrollDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPayrollByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("process")]
    [Authorize(Roles = "Admin,Financial")]
    [ProducesResponseType(typeof(PayrollResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Process([FromBody] ProcessPayrollRequest request, CancellationToken ct)
    {
        var command = new ProcessPayrollCommand(request.Month, request.Year, request.EmployeeIds);
        var result = await _mediator.Send(command, ct);
        if (_notifications.HasNotifications) return BadRequest(_notifications.Notifications);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string reason, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelPayrollCommand(id, reason), ct);
        if (_notifications.HasNotifications) return BadRequest(_notifications.Notifications);
        return result ? NoContent() : NotFound();
    }
}
