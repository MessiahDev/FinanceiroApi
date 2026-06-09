using FinanceiroApi.Application.Commands.BankStatements.CancelBankStatement;
using FinanceiroApi.Application.Commands.BankStatements.ImportBankStatement;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.BankStatements.GetBankStatementById;
using FinanceiroApi.Application.Queries.BankStatements.GetBankStatementsByAccount;
using FinanceiroApi.CrossCutting.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/bank-statements")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class BankStatementsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public BankStatementsController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BankStatementSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? bankAccountId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBankStatementsByAccountQuery(bankAccountId, from, to), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BankStatementResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetBankStatementByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BankStatementResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Import([FromBody] ImportBankStatementRequest request, CancellationToken ct)
    {
        var entries = request.Entries
            .Select(e => new ImportBankStatementEntryCommand(e.Date, e.Description, e.Amount, e.EntryType, e.DocumentNumber))
            .ToList();

        var result = await _mediator.Send(new ImportBankStatementCommand(
            request.BankAccountId,
            request.StatementDate,
            request.PeriodStart,
            request.PeriodEnd,
            request.OpeningBalance,
            request.ClosingBalance,
            request.FileName,
            request.Notes,
            entries), ct);

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(BankStatementSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReasonRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelBankStatementCommand(id, request.Reason), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

