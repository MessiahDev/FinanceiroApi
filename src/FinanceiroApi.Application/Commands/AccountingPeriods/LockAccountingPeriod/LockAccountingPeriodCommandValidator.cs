using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountingPeriods.LockAccountingPeriod;

public class LockAccountingPeriodCommandValidator : AbstractValidator<LockAccountingPeriodCommand>
{
    public LockAccountingPeriodCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("O id do período contábil é obrigatório.");
    }
}
