using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FinanceiroApi.Application.Commands.ChartOfAccounts.CreateChartOfAccount;
using FinanceiroApi.Application.Commands.ChartOfAccounts.UpdateChartOfAccount;
using FinanceiroApi.Application.Commands.ChartOfAccounts.DeactivateChartOfAccount;
using FinanceiroApi.Application.Queries.ChartOfAccounts.GetAllChartOfAccounts;
using FinanceiroApi.Application.Queries.ChartOfAccounts.GetChartOfAccountById;
using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/chart-of-accounts")]
[Authorize]
public class ChartOfAccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChartOfAccountsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] bool? isActive = null,
        [FromQuery] AccountType? accountType = null,
        [FromQuery] bool onlyRoots = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAllChartOfAccountsQuery(isActive, accountType, onlyRoots), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetChartOfAccountByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateChartOfAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateChartOfAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        await _mediator.Send(command with { Id = id }, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new DeactivateChartOfAccountCommand(id), cancellationToken);
        return NoContent();
    }
}
