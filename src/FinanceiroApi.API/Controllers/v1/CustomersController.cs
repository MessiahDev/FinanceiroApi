using FinanceiroApi.Application.Commands.Customers.BlockCustomer;
using FinanceiroApi.Application.Commands.Customers.CreateCustomer;
using FinanceiroApi.Application.Commands.Customers.DeleteCustomer;
using FinanceiroApi.Application.Commands.Customers.UpdateCustomer;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.Customers.GetAllCustomers;
using FinanceiroApi.Application.Queries.Customers.GetCustomerById;
using FinanceiroApi.CrossCutting.Notifications;
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
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public CustomersController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CustomerSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllCustomersQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCustomerByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateCustomerCommand(
                request.Name, request.TaxId, request.PersonType,
                request.Email, request.Phone, request.ContactName, request.CreditLimit), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateCustomerCommand(id, request.Name, request.Email, request.Phone, request.ContactName), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/block")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Block(Guid id, [FromBody] string reason, CancellationToken ct)
    {
        var result = await _mediator.Send(new BlockCustomerCommand(id, reason), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteCustomerCommand(id), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result ? NoContent() : NotFound();
    }
}
