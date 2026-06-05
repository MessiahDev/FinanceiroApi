using FluentValidation;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.UpdateChartOfAccount;

public class UpdateChartOfAccountCommandValidator : AbstractValidator<UpdateChartOfAccountCommand>
{
	public UpdateChartOfAccountCommandValidator()
	{
		RuleFor(x => x.Id)
			.NotEmpty().WithMessage("O id da conta é obrigatório.");

		RuleFor(x => x.Name)
			.NotEmpty().WithMessage("O nome da conta é obrigatório.")
			.MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

		RuleFor(x => x.Description)
			.MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.")
			.When(x => x.Description is not null);
	}
}
