using FluentValidation;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.DeactivateChartOfAccount;

public class DeactivateChartOfAccountCommandValidator : AbstractValidator<DeactivateChartOfAccountCommand>
{
    public DeactivateChartOfAccountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O id da conta é obrigatório.");
    }
}
