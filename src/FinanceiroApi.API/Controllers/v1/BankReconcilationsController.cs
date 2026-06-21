using FinanceiroApi.Application.Commands.BankReconciliations.CreateBankReconciliation;
using FinanceiroApi.Application.Commands.BankReconciliations.CompleteReconciliation;
using FinanceiroApi.Application.Commands.BankReconciliations.CancelReconciliation;
using FinanceiroApi.Application.Commands.BankReconciliations.AddReconciliationItem;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.Queries.BankReconciliations.GetBankReconciliationById;
using FinanceiroApi.Application.Queries.BankReconciliations.GetBankReconciliationsByAccount;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Services;
using FinanceiroApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/bank-reconciliations")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class BankReconciliationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;
    private readonly ICurrentUser _currentUser;
    public BankReconciliationsController(IMediator mediator, INotificationContext notifications, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _notifications = notifications;
        _currentUser = currentUser;
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
            [FromQuery] Guid? bankAccountId,
            [FromQuery] ReconciliationStatus? status,
            CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBankReconciliationsByAccountQuery(bankAccountId, status), ct);
        return Ok(result);
    }
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(
                new GetBankReconciliationByIdQuery(id),
                ct);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
            [FromBody] CreateBankReconciliationCommand command,
            CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(
            Guid id,
            [FromBody] AddReconciliationItemRequest request,
            CancellationToken ct)
    {
        var result = await _mediator.Send(
                new AddReconciliationItemCommand(
                        id,
                        request.BankStatementEntryId,
                        request.TransactionId,
                        request.Amount,
                        request.ItemStatus,
                        request.Notes),
                ct);

        if (result is null)
            return NotFound();

        return Ok(result);
    }
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new CompleteReconciliationCommand(id, _currentUser.UserId), ct);
        return result is null ? NotFound() : Ok(result);
    }
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelReasonRequest request,
            CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelReconciliationCommand(id, request.Reason), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

public record AddReconciliationItemRequest(
        Guid BankStatementEntryId,
        Guid? TransactionId,
        decimal Amount,
        ReconciliationItemStatus ItemStatus,
        string? Notes);
