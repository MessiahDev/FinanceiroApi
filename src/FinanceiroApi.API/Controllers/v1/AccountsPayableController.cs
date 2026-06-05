using FinanceiroApi.Application.Commands.AccountsPayable.CancelAccountPayable;
using FinanceiroApi.Application.Commands.AccountsPayable.CreateAccountPayable;
using FinanceiroApi.Application.Commands.AccountsPayable.PayAccountPayable;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.AccountsPayable.GetAccountPayableById;
using FinanceiroApi.Application.Queries.AccountsPayable.GetAccountsPayable;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/accounts-payable")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class AccountsPayableController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public AccountsPayableController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountPayableResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] AccountPayableStatus? status,
        [FromQuery] Guid? supplierId,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAccountsPayableQuery(status, supplierId), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountPayableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAccountPayableByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountPayableResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAccountPayableRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateAccountPayableCommand(
                request.SupplierId, request.Description, request.TotalAmount,
                request.DueDate, request.CostCenterId, request.InvoiceNumber, request.Notes), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(typeof(AccountPayableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Pay(Guid id, [FromBody] PayAccountPayableRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new PayAccountPayableCommand(id, request.Amount, request.PaymentDate, request.BankAccountId), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(AccountPayableResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] string reason, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelAccountPayableCommand(id, reason), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result is null ? NotFound() : Ok(result);
    }
}
