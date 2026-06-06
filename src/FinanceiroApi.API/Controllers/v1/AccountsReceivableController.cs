using FinanceiroApi.Application.Commands.AccountsReceivable.CancelAccountReceivable;
using FinanceiroApi.Application.Commands.AccountsReceivable.CreateAccountReceivable;
using FinanceiroApi.Application.Commands.AccountsReceivable.ReceivePayment;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.AccountsReceivable.GetOpenReceivables;
using FinanceiroApi.CrossCutting.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/accounts-receivable")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class AccountsReceivableController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public AccountsReceivableController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountReceivableResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] Guid? customerId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOpenReceivablesQuery(customerId), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountReceivableResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAccountReceivableRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateAccountReceivableCommand(
                request.CustomerId, request.Description, request.TotalAmount,
                request.DueDate, request.CostCenterId, request.InvoiceNumber, request.Notes), ct);


        return Created(string.Empty, result);
    }

    [HttpPost("{id:guid}/receive")]
    [ProducesResponseType(typeof(AccountReceivableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Receive(Guid id, [FromBody] ReceivePaymentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ReceivePaymentCommand(id, request.Amount, request.ReceiptDate, request.BankAccountId), ct);


        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(AccountReceivableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReasonRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelAccountReceivableCommand(id, request.Reason), ct);


        return result is null ? NotFound() : Ok(result);
    }
}

