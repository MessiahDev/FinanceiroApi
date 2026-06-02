using FinanceiroApi.Application.Commands.Transactions.CreateTransaction;
using FinanceiroApi.Application.DTOs.Request;
using FinanceiroApi.Application.DTOs.Response;
using FinanceiroApi.CrossCutting.Notifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceiroApi.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly INotificationContext _notifications;

    public TransactionsController(IMediator mediator, INotificationContext notifications)
    {
        _mediator = mediator;
        _notifications = notifications;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        var command = new CreateTransactionCommand(
                        request.Description,
                        request.Amount,
                        request.Type.ToString(),
                        request.Category.ToString(),
                        request.EmployeeId,
                        request.TransactionDate);

        var result = await _mediator.Send(command, ct);
        if (_notifications.HasNotifications) return BadRequest(_notifications.Notifications);

        return Created($"api/v1/transactions/{result.Id}", result);
    }
}
