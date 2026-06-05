using MediatR;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.UpdateChartOfAccount;

public record UpdateChartOfAccountCommand(
    Guid Id,
    string Name,
    string? Description,
    bool AcceptsEntries
) : IRequest;
