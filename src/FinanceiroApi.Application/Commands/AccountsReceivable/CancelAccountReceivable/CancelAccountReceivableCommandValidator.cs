using FluentValidation;

namespace FinanceiroApi.Application.Commands.AccountsReceivable.CancelAccountReceivable;

public class CancelAccountReceivableCommandValidator : AbstractValidator<CancelAccountReceivableCommand>
{
    public CancelAccountReceivableCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty()
            .MaximumLength(500);
    }
}