using MediatR;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.DeactivateChartOfAccount;

public record DeactivateChartOfAccountCommand(Guid Id) : IRequest;