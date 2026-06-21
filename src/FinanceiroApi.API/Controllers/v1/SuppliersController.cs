using FinanceiroApi.Application.Commands.Suppliers.BlockSupplier;
using FinanceiroApi.Application.Commands.Suppliers.CreateSupplier;
using FinanceiroApi.Application.Commands.Suppliers.DeleteSupplier;
using FinanceiroApi.Application.Commands.Suppliers.UpdateSupplier;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.Suppliers.GetAllSuppliers;
using FinanceiroApi.Application.Queries.Suppliers.GetSupplierById;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Pagination;
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
public class SuppliersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public SuppliersController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<SupplierSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetAllSuppliersQuery(pageNumber, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSupplierByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateSupplierCommand(
                request.Name, request.TaxId, request.PersonType,
                request.Email, request.Phone, request.ContactName), ct);


        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateSupplierCommand(id, request.Name, request.Email, request.Phone, request.ContactName), ct);


        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{id:guid}/block")]
    [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Block(Guid id, [FromBody] BlockSupplierRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new BlockSupplierCommand(id, request.Reason), ct);


        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteSupplierCommand(id), ct);


        return result ? NoContent() : NotFound();
    }
}
