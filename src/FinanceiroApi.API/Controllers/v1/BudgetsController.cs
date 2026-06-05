
using FinanceiroApi.Application.Commands.Budgets.ApproveBudget;
using FinanceiroApi.Application.Commands.Budgets.CreateBudget;
using FinanceiroApi.Application.Commands.Budgets.UpdateBudget;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.Budgets.GetBudgetById;
using FinanceiroApi.Application.Queries.Budgets.GetBudgets;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public BudgetsController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BudgetSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? year,
        [FromQuery] BudgetStatus? status,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBudgetsQuery(year, status), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBudgetByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BudgetSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBudgetRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateBudgetCommand(request.Year, request.Name, request.Description), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(BudgetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem(Guid id, [FromBody] AddBudgetItemRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateBudgetCommand(id, request.CostCenterId, request.Category, request.PlannedAmount), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(BudgetSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveBudgetRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new ApproveBudgetCommand(id, request.ApprovedBy), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result is null ? NotFound() : Ok(result);
    }
}
