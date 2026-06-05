using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountingPeriods.CloseAccountingPeriod;

public class CloseAccountingPeriodCommandValidator : AbstractValidator<CloseAccountingPeriodCommand>
{
    public CloseAccountingPeriodCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O id do período contábil é obrigatório.");
    }
}
