using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.Application.Queries.Reports.GetFinancialSummary;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin,Financial")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator) => _mediator = mediator;

    [HttpGet("financial-summary")]
    [ProducesResponseType(typeof(FinancialSummaryResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFinancialSummary(
        [FromQuery] DateOnly periodStart,
        [FromQuery] DateOnly periodEnd,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetFinancialSummaryQuery(periodStart, periodEnd), ct);
        return Ok(result);
    }
}
