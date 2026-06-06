using FinanceiroApi.Application.Commands.Transactions.CancelTransaction;
using FinanceiroApi.Application.Commands.Transactions.ConfirmTransaction;
using FinanceiroApi.Application.Commands.Transactions.CreateTransaction;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
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
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public TransactionsController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var command = new CreateTransactionCommand(
            request.Description,
            request.Amount,
            request.Type.ToString(),
            request.Category.ToString(),
            request.EmployeeId,
            request.TransactionDate);

        var result = await _mediator.Send(command, ct);

        return Created($"api/v1/transactions/{result.Id}", result);
    }

    [HttpPatch("{id:guid}/confirm")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new ConfirmTransactionCommand(id), ct);
        if (result is null) return NotFound();

        return Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelTransactionRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelTransactionCommand(id, request.Reason), ct);
        if (result is null) return NotFound();

        return Ok(result);
    }
}