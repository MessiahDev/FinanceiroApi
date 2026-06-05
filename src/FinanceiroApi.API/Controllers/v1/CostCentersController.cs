using FinanceiroApi.Application.Commands.CostCenters.CreateCostCenter;
using FinanceiroApi.Application.Commands.CostCenters.UpdateCostCenter;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.CostCenters.GetAllCostCenters;
using FinanceiroApi.CrossCutting.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/cost-centers")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class CostCentersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public CostCentersController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CostCenterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllCostCentersQuery(), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CostCenterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCostCenterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateCostCenterCommand(
                request.Code, request.Name, request.AnnualBudget,
                request.ParentId, request.ManagerId, request.Description), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return Created(string.Empty, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CostCenterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCostCenterRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateCostCenterCommand(id, request.Code, request.Name, request.Description, request.ManagerId), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result is null ? NotFound() : Ok(result);
    }
}
