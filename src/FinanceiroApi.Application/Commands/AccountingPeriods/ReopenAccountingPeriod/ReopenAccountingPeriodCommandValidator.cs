using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountingPeriods.ReopenAccountingPeriod;

public class ReopenAccountingPeriodCommandValidator : AbstractValidator<ReopenAccountingPeriodCommand>
{
    public ReopenAccountingPeriodCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O id do período contábil é obrigatório.");
    }
}
