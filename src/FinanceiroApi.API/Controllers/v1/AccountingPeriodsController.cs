using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceiroApi.Application.Commands.AccountingPeriods.CreateAccountingPeriod;
using FinanceiroApi.Application.Commands.AccountingPeriods.CloseAccountingPeriod;
using FinanceiroApi.Application.Commands.AccountingPeriods.LockAccountingPeriod;
using FinanceiroApi.Application.Commands.AccountingPeriods.ReopenAccountingPeriod;
using FinanceiroApi.Application.Queries.AccountingPeriods.GetAllAccountingPeriods;
using FinanceiroApi.Application.Queries.AccountingPeriods.GetAccountingPeriodById;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/accounting-periods")]
[Authorize]
public class AccountingPeriodsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountingPeriodsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
    [FromQuery] int? year = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAllAccountingPeriodsQuery(year, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAccountingPeriodByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAccountingPeriodCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new CloseAccountingPeriodCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/lock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lock(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new LockAccountingPeriodCommand(id), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new ReopenAccountingPeriodCommand(id), cancellationToken);
        return NoContent();
    }
}