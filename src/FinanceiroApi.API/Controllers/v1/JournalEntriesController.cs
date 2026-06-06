using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceiroApi.Application.Commands.JournalEntries.CreateJournalEntry;
using FinanceiroApi.Application.Commands.JournalEntries.PostJournalEntry;
using FinanceiroApi.Application.Commands.JournalEntries.ReverseJournalEntry;
using FinanceiroApi.Application.Queries.JournalEntries.GetJournalEntryById;
using FinanceiroApi.Application.Queries.JournalEntries.GetJournalEntriesByPeriod;
using FinanceiroApi.Application.Queries.Accounting.GetTrialBalance;
using FinanceiroApi.CrossCutting.Services;
using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/journal-entries")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;

    public JournalEntriesController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPeriod(
        [FromQuery] Guid accountingPeriodId,
        [FromQuery] JournalEntryStatus? status = null,
        [FromQuery] JournalEntryType? entryType = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetJournalEntriesByPeriodQuery(accountingPeriodId, status, entryType), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetJournalEntryByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateJournalEntryCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/post")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new PostJournalEntryCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reverse")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reverse(
        Guid id,
        [FromBody] ReverseJournalEntryRequest request,
        CancellationToken cancellationToken = default)
    {
        var reversalId = await _mediator.Send(
            new ReverseJournalEntryCommand(id, request.Description, _currentUser.UserId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = reversalId }, new { id = reversalId });
    }

    [HttpGet("trial-balance")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] Guid accountingPeriodId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetTrialBalanceQuery(accountingPeriodId), cancellationToken);
        return Ok(result);
    }
}

public record ReverseJournalEntryRequest(string Description);
