using FinanceiroApi.Application.Commands.TaxEntries.CancelTaxEntry;
using FinanceiroApi.Application.Commands.TaxEntries.CreateTaxEntry;
using FinanceiroApi.Application.Commands.TaxPayments.CancelTaxPayment;
using FinanceiroApi.Application.Commands.TaxPayments.CreateTaxPayment;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.TaxEntries.GetOverdueTaxEntries;
using FinanceiroApi.Application.Queries.TaxEntries.GetTaxEntries;
using FinanceiroApi.Application.Queries.TaxEntries.GetTaxEntryById;
using FinanceiroApi.Application.Queries.TaxPayments.GetTaxPaymentById;
using FinanceiroApi.Application.Queries.TaxPayments.GetTaxPaymentsByEntry;
using FinanceiroApi.CrossCutting.Notifications;
using FinanceiroApi.CrossCutting.Pagination;
using FinanceiroApi.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/tax-entries")]
[Authorize]
[EnableRateLimiting("general")]
[Produces("application/json")]
public class TaxEntriesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public TaxEntriesController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaxEntrySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
    [FromQuery] TaxType? taxType,
    [FromQuery] TaxEntryStatus? status,
    [FromQuery] int? competenceYear,
    [FromQuery] int? competenceMonth,
    [FromQuery] DateOnly? dueDateFrom,
    [FromQuery] DateOnly? dueDateTo,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetTaxEntriesQuery(taxType, status, competenceYear, competenceMonth, dueDateFrom, dueDateTo, pageNumber, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("overdue")]
    [ProducesResponseType(typeof(IReadOnlyList<TaxEntrySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOverdueTaxEntriesQuery(), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaxEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTaxEntryByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaxEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTaxEntryRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateTaxEntryCommand(
            request.TaxType,
            request.Description,
            request.BaseAmount,
            request.Rate,
            request.Competence,
            request.DueDate,
            request.CostCenterId,
            request.ReferenceDocument,
            request.ReferenceDocumentId,
            request.Notes), ct);

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(TaxEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelReasonRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelTaxEntryCommand(id, request.Reason), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/payments")]
    [ProducesResponseType(typeof(IReadOnlyList<TaxPaymentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTaxPaymentsByEntryQuery(id), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/payments/{paymentId:guid}")]
    [ProducesResponseType(typeof(TaxPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(Guid id, Guid paymentId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTaxPaymentByIdQuery(paymentId), ct);
        return Ok(result);
    }

    [HttpPost("{id:guid}/payments")]
    [ProducesResponseType(typeof(TaxPaymentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterPayment(Guid id, [FromBody] CreateTaxPaymentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateTaxPaymentCommand(
            id,
            request.BankAccountId,
            request.Amount,
            request.PaymentDate,
            request.Fine,
            request.Interest,
            request.DarfNumber,
            request.ReceiptCode,
            request.Notes), ct);

        return result is null ? NotFound() : CreatedAtAction(nameof(GetPaymentById), new { id, paymentId = result.Id }, result);
    }

    [HttpPatch("{id:guid}/payments/{paymentId:guid}/cancel")]
    [ProducesResponseType(typeof(TaxPaymentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPayment(Guid id, Guid paymentId, [FromBody] CancelTaxPaymentRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new CancelTaxPaymentCommand(paymentId, request.Reason), ct);
        return result is null ? NotFound() : Ok(result);
    }
}

