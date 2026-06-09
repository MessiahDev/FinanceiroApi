using FinanceiroApi.Application.Commands.BankReconciliations.CreateBankReconciliation;
using FinanceiroApi.Application.Commands.BankReconciliations.CompleteReconciliation;
using FinanceiroApi.Application.Commands.BankReconciliations.CancelReconciliation;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.Queries.BankReconciliations.GetBankReconciliationById;
using FinanceiroApi.Application.Queries.BankReconciliations.GetBankReconciliationsByAccount;
using FinanceiroApi.CrossCutting.Notifications;
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

	public BankReconciliationsController(IMediator mediator, INotificationContext notifications)
	{
		_mediator = mediator;
		_notifications = notifications;
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

	[HttpPost("{id:guid}/complete")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
	{
		var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
		var result = await _mediator.Send(new CompleteReconciliationCommand(id, userId), ct);

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
