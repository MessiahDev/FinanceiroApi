using FinanceiroApi.Application.Commands.BankAccounts.CreateBankAccount;
using FinanceiroApi.Application.Commands.BankAccounts.TransferBetweenAccounts;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.BankAccounts.GetAllBankAccounts;
using FinanceiroApi.Application.Queries.BankAccounts.GetBankAccountById;
using FinanceiroApi.CrossCutting.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/bank-accounts")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class BankAccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public BankAccountsController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BankAccountResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllBankAccountsQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BankAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBankAccountByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BankAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBankAccountRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new CreateBankAccountCommand(
                request.BankName, request.BankCode, request.Agency,
                request.AccountNumber, request.AccountType,
                request.InitialBalance, request.PixKey, request.Description), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpPost("transfer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Transfer([FromBody] TransferBetweenAccountsRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new TransferBetweenAccountsCommand(
                request.SourceAccountId, request.DestinationAccountId,
                request.Amount, request.Description), ct);

        if (_notifications.HasNotifications)
            return BadRequest(_notifications.Notifications);

        return result ? NoContent() : BadRequest();
    }
}
