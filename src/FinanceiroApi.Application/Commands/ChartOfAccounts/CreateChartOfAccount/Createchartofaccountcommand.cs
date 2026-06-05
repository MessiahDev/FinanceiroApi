using MediatR;
using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.CreateChartOfAccount;

public record CreateChartOfAccountCommand(
	string Code,
	string Name,
	string? Description,
	AccountType AccountType,
	AccountNature AccountNature,
	bool AcceptsEntries,
	Guid? ParentAccountId
) : IRequest<Guid>;
