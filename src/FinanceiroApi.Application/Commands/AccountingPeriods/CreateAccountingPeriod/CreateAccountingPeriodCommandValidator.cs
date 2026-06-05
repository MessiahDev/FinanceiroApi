using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountingPeriods.CreateAccountingPeriod;

public class CreateAccountingPeriodCommandValidator : AbstractValidator<CreateAccountingPeriodCommand>
{
    public CreateAccountingPeriodCommandValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Ano inválido (deve ser entre 2000 e 2100).");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Mês inválido (deve ser entre 1 e 12).");
    }
}
