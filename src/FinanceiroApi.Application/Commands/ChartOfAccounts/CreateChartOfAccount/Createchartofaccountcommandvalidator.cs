using FluentValidation;
using FinanceiroApi.Domain.Enums;

namespace FinanceiroApi.Application.Commands.ChartOfAccounts.CreateChartOfAccount;

public class CreateChartOfAccountCommandValidator : AbstractValidator<CreateChartOfAccountCommand>
{
    public CreateChartOfAccountCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("O código da conta é obrigatório.")
            .MaximumLength(20).WithMessage("O código deve ter no máximo 20 caracteres.")
            .Matches(@"^[\d.]+$").WithMessage("O código deve conter apenas dígitos e pontos (ex: 1.1.01.001).");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome da conta é obrigatório.")
            .MaximumLength(150).WithMessage("O nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("A descrição deve ter no máximo 500 caracteres.")
            .When(x => x.Description is not null);

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage("Tipo de conta inválido.");

        RuleFor(x => x.AccountNature)
            .IsInEnum().WithMessage("Natureza da conta inválida.");
    }
}